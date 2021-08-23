using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState { Idle, Move }

public class Unit : MonoBehaviour
{
    public event System.Action<HexCoordinates, HexCoordinates> onMove = (HexCoordinates from, HexCoordinates to) => { };
    public event System.Action onMoveStart = () => { };
    public event System.Action onMoveFinish = () => { };
    public event System.Action onDestroy = () => { };
    public event System.Action onSpawn = () => { };
    public event System.Action onAction = () => { };
    public event System.Action onRecover = () => { };

    [SerializeField]
    UnitArchitype unitArchitype;

    [SerializeField]
    Transform unitGridIndicator;

    int maxActionCount = 2;

    PlayerIdentifier owner = PlayerIdentifier.Player0;

    Ability[] abilities;

    int remainingActionCount = 0;

    bool isEarlyCreated = false;

    SubUnitMovement[] subUnits;

    Banner banner;

    UnitState state = UnitState.Idle;

    int maxSubUnitsCount = 1;

    int aliveSubUnitsCount = 1;

    Queue<HexCoordinates> path = new Queue<HexCoordinates>();

    HexCoordinates currentHex = new HexCoordinates(0, 0);

    float subUnitMovingDelay = 0.2f;

    public int Speed
    {
        get => unitArchitype.Speed;
    }

    public int RemainingActionCount { get => remainingActionCount; }

    public PlayerIdentifier Owner
    {
        get => owner;
        set => owner = value;
    }

    public AbilityType[] SetOfAbilities
    {
        get => unitArchitype.SetOfAbbilities;
    }

    public Ability[] AbilityInstances
    {
        get => abilities;
    }

    public HexCoordinates CurrentHex
    {
        get => currentHex;
        set
        {
            currentHex = value;
            if (subUnits != null && subUnits.Length > 0)
            {
                ClearSubUnits();
                CreateSubUnits();
            }
            UpdateGridIndicatorPosition();
        }
    } 

    public UnitState State { get => state; }

    public bool ShowIndicator
    {
        get => unitGridIndicator.gameObject.activeSelf;
        set => unitGridIndicator.gameObject.SetActive(value);
    }

    public void Destroy()
    {
        HexMapMaster.Instance.DestroyUnit(currentHex);
        onDestroy.Invoke();
        Destroy(gameObject);
    }

    public void KillSubUnit()
    {
        if (aliveSubUnitsCount > 0)
        {
            aliveSubUnitsCount--;
        }
    }

    public void ResurrectSubUnit()
    {
        if (aliveSubUnitsCount < maxSubUnitsCount)
        {
            aliveSubUnitsCount++;
        }
    }

    public void UseAbility(int index)
    {
        if (abilities.Length > index)
        {
            abilities[index].Use();
        }
        onAction();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!GameMaster.Instance.GameStarted)
        {
            GameMaster.onGameStart += Init;
            isEarlyCreated = true;
        }
        else
            Init();
    }

    void Recover()
    {
        remainingActionCount = maxActionCount;
        onRecover();
    }

    private void Init()
    {
        maxSubUnitsCount = aliveSubUnitsCount = unitArchitype.SubunitsPrefabs.Length;
        CreateSubUnits();
        UpdateGridIndicatorPosition();
        banner = Instantiate(unitArchitype.BannerPrefab);
        banner.SetOwner(subUnits[0].transform);
        banner.transform.SetParent(transform, false);

        List<Ability> tempAbility = new List<Ability>();
        foreach (AbilityType type in unitArchitype.SetOfAbbilities)
        {
            var newAbility = AbilityCreator.CreateAbility(type);
            newAbility.owner = this;
            tempAbility.Add(newAbility);
        }
        abilities = tempAbility.ToArray();

        GameMaster.onTurnStart += Recover;
        Recover();

        if (isEarlyCreated)
            GameMaster.onGameStart -= Init;
    }

    private void OnDestroy()
    {
        subUnits[0].onDestinationReached -= UpdatePath;
        GameMaster.onTurnStart -= Recover;
    }

    void CreateSubUnits()
    {
        List<SubUnitMovement> subUnitsList = new List<SubUnitMovement>();
        for (int i = 0; i < aliveSubUnitsCount; i++)
        {
            SubUnitMovement subUnit = unitArchitype.SubunitsPrefabs[i];
            SubUnitMovement instance = Instantiate(subUnit);
            instance.transform.position = HexMapMaster.Instance.GetCellOnMap(currentHex).transform.localPosition + BattleFormations.FormationOffsets(unitArchitype.BattleFormation)[i];
            instance.transform.SetParent(transform, false);
            subUnitsList.Add(instance);
        }
        subUnits = subUnitsList.ToArray();
        subUnits[0].onDestinationReached += UpdatePath;
    }

    void ClearSubUnits()
    {
        subUnits[0].onDestinationReached -= UpdatePath;
        foreach (SubUnitMovement subUnit in subUnits)
        {
            Destroy(subUnit.gameObject);
        }
    }

    public bool IsReachable(HexCoordinates hex)
    {

        return HexMapMaster.Instance.FindPath(currentHex, hex, out _, remainingActionCount * unitArchitype.Speed).Length != 0;
    }

    public void GoTo(HexCoordinates destination)
    {
        int[] cost;
        HexCoordinates[] path = HexMapMaster.Instance.FindPath(currentHex, destination, out cost, unitArchitype.Speed * 2);

        if (path.Length == 0)
            return;

        foreach (HexCoordinates hex in path)
        {
            this.path.Enqueue(hex);
        }

        Vector3[,] subUnitsPathes = new Vector3[maxSubUnitsCount, path.Length];

        for (int pi = 0; pi < path.Length; pi++)
        {
            if (pi < path.Length - 1)
            {
                HexCoordinates current = path[pi];
                HexCoordinates next = path[pi + 1];
                Vector3 currentPos = HexMapMaster.Instance.GetCellOnMap(current).transform.position;
                Vector3 nextPos = HexMapMaster.Instance.GetCellOnMap(next).transform.position;
                Vector3 pathPoint = Vector3.Lerp(currentPos, nextPos, 0.5f);
                for (int si = 0; si < maxSubUnitsCount; si++)
                {
                    subUnitsPathes[si, pi] = pathPoint;
                }
            }
            else
            {
                HexCoordinates current = path[pi];
                HexCoordinates previous = path[pi - 1];
                Vector3 previousPos = HexMapMaster.Instance.GetCellOnMap(previous).transform.position;
                Vector3 currentPos = HexMapMaster.Instance.GetCellOnMap(current).transform.position;
                Vector3 pathPoint = currentPos;

                float angle = Quaternion.LookRotation(currentPos - previousPos).eulerAngles.y;

                for (int si = 0; si < maxSubUnitsCount; si++)
                {
                    subUnitsPathes[si, path.Length - 1] = pathPoint + BattleFormations.FormationOffsets(unitArchitype.BattleFormation, angle)[si];
                }
            }
        }

        for (int si = 0; si < maxSubUnitsCount; si++)
        {
            StartCoroutine(SetSubUnitPathWithDelay(si, subUnitsPathes.GetRow(si), subUnitMovingDelay * si));
        }

        remainingActionCount--;
        if (cost[cost.Length - 1] > unitArchitype.Speed)
        {
            remainingActionCount--;
        }
        onAction();
        state = UnitState.Move;
        onMoveStart.Invoke();
    }

    IEnumerator SetSubUnitPathWithDelay(int subUnitIndex, Vector3[] path, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetSubUnitPath(subUnitIndex, path);
    }

    void SetSubUnitPath(int subUnitIndex, Vector3[] path)
    {
        foreach (Vector3 pathPoint in path)
        {
            subUnits[subUnitIndex].AddCheckpoint(pathPoint);
        }
    }

    void UpdateGridIndicatorPosition()
    {
        if (unitGridIndicator)
            unitGridIndicator.position = HexMapMaster.Instance.GetCellOnMap(currentHex).transform.position;
    }

    void UpdatePath()
    {
        if (path.Count > 0)
        {
            HexCoordinates previousHex = currentHex;
            currentHex = path.Dequeue();
            onMove.Invoke(previousHex, currentHex);
        }
        if (path.Count == 0 && state != UnitState.Idle)
        {
            state = UnitState.Idle;
            onMoveFinish.Invoke();
        }
        UpdateGridIndicatorPosition();
    }
}
