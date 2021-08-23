using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    int x, z;

    public int X { get => x; }
    public int Z { get => z; }

    public int Y { get => -X - Z; }

    public HexCoordinates (int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (GameDataPresenter.Instance.HexMetrics.InnerRadius * 2f);
        float y = -x;
        float offset = position.z / (GameDataPresenter.Instance.HexMetrics.OuterRadius * 3f);
        x -= offset;
        y -= offset;
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + ")";
    }

    public string ToVerticalString()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }

    public override bool Equals(object obj)
    {
        HexCoordinates other = (HexCoordinates)obj;
        return x == other.x && Y == other.Y && Z == other.Z;
    }

    public static bool operator ==(HexCoordinates a, HexCoordinates b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(HexCoordinates a, HexCoordinates b)
    {
        return !a.Equals(b);
    }

    public HexCoordinates GetNeighbor(HexDirection direction)
    {
        if (direction == HexDirection.NE)
        {
            return new HexCoordinates(X, Z + 1);
        }
        else if (direction == HexDirection.E)
        {
            return new HexCoordinates(X + 1, Z);
        }
        else if (direction == HexDirection.SE)
        {
            return new HexCoordinates(X + 1, Z - 1);
        }
        else if (direction == HexDirection.SW)
        {
            return new HexCoordinates(X, Z - 1);
        }
        else if (direction == HexDirection.W)
        {
            return new HexCoordinates(X - 1, Z);
        }
        else
        {
            return new HexCoordinates(X - 1, Z + 1);
        }
    }
}
