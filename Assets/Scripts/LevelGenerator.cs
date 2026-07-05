using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LevelGrid
{
    public Vector2 gridMin = new Vector2(-4.5f, 3.25f);
    public Vector2 gridMax = new Vector2(4.5f, 11.25f);
    public Vector2 cellSize = new Vector2(1f, 0.5f);

    public int Columns => Mathf.RoundToInt((gridMax.x - gridMin.x) / cellSize.x);
    public int Rows => Mathf.RoundToInt((gridMax.y - gridMin.y) / cellSize.y);

    public Vector2 CellCenter(int col, int row)
    {
        float x = gridMin.x + cellSize.x * 0.5f + col * cellSize.x;
        float y = gridMax.y - cellSize.y * 0.5f - row * cellSize.y;
        return new Vector2(x, y);
    }
}

public enum BrickType { None = -1, Green, Blue, Yellow, Red, Metal }

public class LevelGenerator : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private LevelGrid grid = new LevelGrid();

    [Header("Brick Prefabs")]
    [SerializeField] private Brick greenBrickPrefab;
    [SerializeField] private Brick blueBrickPrefab;
    [SerializeField] private Brick yellowBrickPrefab;
    [SerializeField] private Brick redBrickPrefab;
    [SerializeField] private Brick metalBrickPrefab;

    [Header("Difficulty Scaling")]
    [SerializeField] private int baseRows = 5;
    [SerializeField] private int maxRows = 15;

    [Header("Procedural Rules")]
    [SerializeField, Range(0f, 1f)] private float baseGapChance = 0.05f;
    [SerializeField, Range(0f, 0.05f)] private float gapChanceStep = 0.02f;
    [SerializeField, Range(0f, 1f)] private float metalChance = 0.5f; // chance a ceiling-exposed brick becomes metal
    [SerializeField] private int caveStartLevel = 4; // levels at/above this use cave-style generation
    private const int MinVerticalGapRows = 3; // matches the 16x24px minimum bounce space

    // --- Public entry point ---

    public void Generate(int levelNumber)
    {
        ClearExisting();

        BrickType[,] bricks = levelNumber == 0
            ? BuildFixedLevel1()
            : BuildProceduralLevel(levelNumber);

        PopulateGrid(bricks);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private int debugLevelNumber = 0;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            Generate(debugLevelNumber);
            Debug.Log($"Regenerated level {debugLevelNumber}");
        }
    }
#endif

    private void ClearExisting()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    // --- How many rows of vertical space are available to populate this level ---

    private int GetAvailableRows(int levelNumber)
    {
        return Mathf.Min(baseRows + levelNumber, maxRows, grid.Rows);
    }

    // --- Level 1: fixed, classic layout (bottom to top: green, blue, yellow, red, metal) ---

    private BrickType[,] BuildFixedLevel1()
    {
        BrickType[,] bricks = new BrickType[grid.Columns, grid.Rows];
        FillAll(bricks, BrickType.None);

        int rows = grid.Rows;

        // row order from the bottom of the block upward: green, blue, yellow, red, metal
        BrickType[] rowOrder = { BrickType.Metal, BrickType.Red, BrickType.Yellow, BrickType.Blue, BrickType.Green };

        for (int i = 0; i < rowOrder.Length; i++)
        {
            int row = rows - rowOrder.Length + i;

            for (int col = 0; col < grid.Columns; col++)
                bricks[col, row] = rowOrder[i];
        }

        return bricks;
    }

    // --- Procedural: random colors, growing available space, cave shapes at higher levels ---

    private BrickType[,] BuildProceduralLevel(int levelNumber)
    {
        int rows = GetAvailableRows(levelNumber);
        int rowStart = grid.Rows - rows;
        int rowEnd = grid.Rows;

        bool[,] filled = levelNumber < caveStartLevel
            ? GenerateRows(rowStart, rowEnd, levelNumber)
            : GenerateCave(rowStart, rowEnd, levelNumber);

        return AssignColors(filled, rowStart, rowEnd);
    }

    private bool[,] GenerateRows(int rowStart, int rowEnd, int levelNumber)
    {
        bool[,] filled = new bool[grid.Columns, grid.Rows];
        float gapChance = Mathf.Clamp01(baseGapChance + levelNumber * gapChanceStep);

        for (int col = 0; col < grid.Columns; col++)
            for (int row = rowStart; row < rowEnd; row++)
                filled[col, row] = Random.value > gapChance;

        return filled;
    }

    private bool[,] GenerateCave(int rowStart, int rowEnd, int levelNumber)
    {
        float fillChance = Mathf.Clamp01(0.55f + levelNumber * 0.01f);

        bool[,] cells = new bool[grid.Columns, grid.Rows];

        for (int col = 0; col < grid.Columns; col++)
            for (int row = rowStart; row < rowEnd; row++)
                cells[col, row] = Random.value < fillChance;

        for (int i = 0; i < 3; i++)
            cells = SmoothStep(cells, rowStart, rowEnd);

        CloseTinyVerticalGaps(cells, rowStart, rowEnd);

        return cells;
    }

    private bool[,] SmoothStep(bool[,] cells, int rowStart, int rowEnd)
    {
        bool[,] result = new bool[grid.Columns, grid.Rows];

        for (int col = 0; col < grid.Columns; col++)
        {
            for (int row = rowStart; row < rowEnd; row++)
            {
                int neighbors = CountFilledNeighbors(cells, col, row, rowStart, rowEnd);
                result[col, row] = neighbors > 4 ? true : neighbors < 4 ? false : cells[col, row];
            }
        }

        return result;
    }

    private int CountFilledNeighbors(bool[,] cells, int col, int row, int rowStart, int rowEnd)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int c = col + dx;
                int r = row + dy;

                if (c < 0 || c >= grid.Columns || r < rowStart || r >= rowEnd)
                {
                    count++; // treat out-of-bounds as "filled" so edges don't erode away
                    continue;
                }

                if (cells[c, r])
                    count++;
            }
        }

        return count;
    }

    private void CloseTinyVerticalGaps(bool[,] cells, int rowStart, int rowEnd)
    {
        for (int col = 0; col < grid.Columns; col++)
        {
            int gapStart = -1;

            for (int row = rowStart; row <= rowEnd; row++)
            {
                bool isFilled = row < rowEnd && cells[col, row];

                if (!isFilled)
                {
                    if (gapStart == -1)
                        gapStart = row;
                    continue;
                }

                int gapLength = row - gapStart;
                if (gapStart != -1 && gapLength > 0 && gapLength < MinVerticalGapRows)
                    for (int r = gapStart; r < row; r++)
                        cells[col, r] = true; // gap too tight for a clean bounce, seal it

                gapStart = -1;
            }
        }
    }

    // --- Turns filled/empty cells into actual colored bricks: random mix, metal only on ceiling-exposed cells ---

    private BrickType[,] AssignColors(bool[,] filled, int rowStart, int rowEnd)
    {
        BrickType[,] bricks = new BrickType[grid.Columns, grid.Rows];
        FillAll(bricks, BrickType.None);

        for (int col = 0; col < grid.Columns; col++)
        {
            for (int row = rowStart; row < rowEnd; row++)
            {
                if (!filled[col, row])
                    continue;

                bool exposedToCeiling = IsExposedToCeiling(filled, col, row, rowStart);
                bool isMetal = exposedToCeiling && Random.value < metalChance;

                bricks[col, row] = isMetal ? BrickType.Metal : RandomColor();
            }
        }

        return bricks;
    }

    private BrickType RandomColor()
    {
        int roll = Random.Range(0, 4);
        return (BrickType)roll; // Green, Blue, Yellow, Red are 0-3 in the enum
    }

    private bool IsExposedToCeiling(bool[,] filled, int col, int row, int rowStart)
    {
        for (int r = rowStart; r < row; r++)
            if (filled[col, r])
                return false; // something above it, not directly exposed

        return true;
    }

    private void FillAll(BrickType[,] bricks, BrickType value)
    {
        for (int col = 0; col < grid.Columns; col++)
            for (int row = 0; row < grid.Rows; row++)
                bricks[col, row] = value;
    }

    // --- Hand-authored shape masks (optional, e.g. bonus levels) ---

    public void GenerateFromMask(string[] mask)
    {
        ClearExisting();

        bool[,] filled = new bool[grid.Columns, grid.Rows];
        int rows = Mathf.Min(mask.Length, grid.Rows);

        for (int row = 0; row < rows; row++)
        {
            string line = mask[row];
            int cols = Mathf.Min(line.Length, grid.Columns);

            for (int col = 0; col < cols; col++)
                filled[col, row] = line[col] == 'X';
        }

        BrickType[,] bricks = AssignColors(filled, 0, grid.Rows);
        PopulateGrid(bricks);
    }

    // --- Fills the grid with actual brick GameObjects ---

    private void PopulateGrid(BrickType[,] bricks)
    {
        for (int col = 0; col < grid.Columns; col++)
        {
            for (int row = 0; row < grid.Rows; row++)
            {
                BrickType type = bricks[col, row];

                if (type == BrickType.None)
                    continue;

                Brick prefab = GetPrefab(type);
                if (prefab == null)
                    continue;

                Instantiate(prefab, grid.CellCenter(col, row), Quaternion.identity, transform);
            }
        }
    }

    private Brick GetPrefab(BrickType type)
    {
        switch (type)
        {
            case BrickType.Green: return greenBrickPrefab;
            case BrickType.Blue: return blueBrickPrefab;
            case BrickType.Yellow: return yellowBrickPrefab;
            case BrickType.Red: return redBrickPrefab;
            case BrickType.Metal: return metalBrickPrefab;
            default: return null;
        }
    }

    // --- Scene view grid visualization ---

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        for (int col = 0; col <= grid.Columns; col++)
        {
            float x = grid.gridMin.x + col * grid.cellSize.x;
            Gizmos.DrawLine(new Vector3(x, grid.gridMin.y), new Vector3(x, grid.gridMax.y));
        }

        for (int row = 0; row <= grid.Rows; row++)
        {
            float y = grid.gridMax.y - row * grid.cellSize.y;
            Gizmos.DrawLine(new Vector3(grid.gridMin.x, y), new Vector3(grid.gridMax.x, y));
        }
    }
}