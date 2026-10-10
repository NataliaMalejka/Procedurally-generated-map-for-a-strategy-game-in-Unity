using UnityEngine;

// Represents a single chunk of hex cells
public class Chunk : MonoBehaviour
{
    [SerializeField] private HexCell cellPrefab;
    // All cells belonging to this chunk
    private HexCell[] cells;
    // Number of cells
    private int xCells;
    private int zCells;

    // Vertical layer index
    private int level = 0;
    // Index for water color 
    private int waterColorIndex = 1;

    // Column index for wrap 
    private int columnIndex = -1;

    // Mesh generator for this chunk
    [SerializeField] private ChunkMesh chunkMesh;

    public HexCell[] GetCells()
    {
       return cells;
    }

    public int GetXCellCount()
    {
        return xCells;
    }

    public int GetZCellCount()
    {
        return zCells;
    }

    // Sets vertical level 
    public void SetLevel(int l)
    {
        level = l;
        gameObject.layer = LayerMask.NameToLayer($"Layer_{level}");
    }

    // Sets water color index used by mesh generation
    public void SetWaterColorIndex(int index)
    {
        waterColorIndex = index;
    }

    // Sets the column index this chunk belongs to
    public void SetColumnIndex(int index)
    {
        columnIndex = index;
    }

    // Initializes chunk size and allocates cell array
    private void OnEnable()
    {
        xCells = MapManager.Instance.xCellCount;
        zCells = MapManager.Instance.zCellCount;

        cells = new HexCell[xCells * zCells];
    }

    // Creates a single cell inside this chunk
    public HexCell CreateCell(int cellGlobalIndex, int chunkIndex)
    {
        // Position cell relative to chunk
        int localCellIndex = cellGlobalIndex - (chunkIndex * xCells * zCells);

        HexCell cell = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform);

        cells[localCellIndex] = cell;
        cell.HexChunk = this;

        // Position cell relative to chunk
        SetCellPosition(cell, localCellIndex, chunkIndex % MapManager.Instance.zChunkCount);

        return cell;
    }

    // Calculates and sets the local position of a cell within the chunk
    private void SetCellPosition(HexCell cell, int index, int zChunkIndex)
    {
        Vector3 position;

        // Base grid position
        position.x = (index / zCells) * HexData.distanceToEdge * 2f;
        position.y = cell.CentreTerrainLevel;
        position.z = (index % zCells) * HexData.distanceToCorner * 1.5f;

        // Offset every second row for hex layout
        int rowIndex = (index % zCells) % 2;
        int offset = (zCells % 2 == 0) ? 1 : 1 - (zChunkIndex % 2);

        if (rowIndex == offset)
        {
            position.x += HexData.distanceToEdge;
        }

        cell.transform.localPosition = position;
    }

    // Generates mesh data for a single hex cell
    private void CreateCellMesh(HexCell cell)
    {
        Vector3 centre = new Vector3(
            cell.transform.localPosition.x,
            cell.CentreTerrainLevel,
            cell.transform.localPosition.z
        );

        int t = cell.TextureIndex;

        // Iterate through all 6 edges of the hex
        for (int i = 0; i < 6; i++)
        {
            Vector3 v1 = cell.GetEdge(i).GetLocalV1();
            Vector3 v2 = cell.GetEdge(i).GetLocalV2();

            Vector3 middle1 = cell.GetEdge(i).GetMiddle1();
            Vector3 middle2 = cell.GetEdge(i).GetMiddle2();

            HexCell neighborCell = cell.GetNeighbor((HexDirection)i);

            // Create terrain triangles
            if (neighborCell != null && cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth)
            {
                chunkMesh.CreateSmoothTriangleWithColor(centre, v1, v2, middle1, middle2, t, cell);
            }
            else
            {
                chunkMesh.CreateTriangleWithColor(centre, v1, v2, t, cell, i);
            }

            // Handle rivers
            if (cell.GetEdge(i).InRiver)
            {
                chunkMesh.CreateHexRiver(middle1, middle2, cell, cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth, centre, i);
            }
            else if (cell.GetEdge(i).OutRiver)
            {
                // Check if any incoming river exists
                bool hasInRiver = false;

                for (int j = 0; j < 6; j++)
                {
                    if (cell.GetEdge(j).InRiver)
                    {
                        hasInRiver = true;
                        break;
                    }
                }

                // Create river source or end if needed
                if (!hasInRiver)
                {
                    chunkMesh.CreateRiverSourceOrEnd(middle1, middle2, cell, cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth, centre);
                }
            }

            // Create rectangle connections 
            if (i < 3)
            {               
                if (neighborCell!= null)
                {
                    if (cell.GetEdge(i).GetEdgeType() == EdgeType.Smooth)
                    {
                        chunkMesh.CreateSmoothConnection(v1, v2, i, t, cell, neighborCell, middle1, middle2);
                    }
                    else
                        chunkMesh.CreateRectangularCellsConnection(v1, v2, i, t, neighborCell, middle1, middle2, cell);

                    // Create triangle connection
                    if (i < 2 && cell.GetNeighbor((HexDirection)i + 1) != null)
                    {
                        HexCell nextNeighborCell = cell.GetNeighbor((HexDirection)i+1);

                        if (nextNeighborCell!=null)
                        {
                            chunkMesh.CreateTriangleCellsConnection(v2, i, t, cell ,neighborCell, nextNeighborCell);
                        }                      
                    }
                }              
            }
        }

        // Add structure if present
        if (cell.StructureIndex >= 0)
        {
            CreateStructure(cell, centre);
        }

        // Add unit if present
        if (cell.IsUnit)
        {
            CreateUnits(cell, centre);
        }
    }

    // Instantiates a structure prefab on the given cell
    private void CreateStructure(HexCell cell, Vector3 pos)
    {
        GameObject[] structures = MapManager.Instance.GetObjects(cell.GetBiome());

        pos = chunkMesh.AddNoise(pos, 3);

        Vector3 centre = new Vector3(
            pos.x + this.transform.position.x,
            pos.y + this.transform.position.y,
            pos.z + this.transform.position.z
        );

        if (structures == null || structures.Length == 0)
            return;

        GameObject cellStructure = structures[cell.StructureIndex];
        Vector3 baseEuler = cellStructure.transform.eulerAngles;

        Quaternion finalRotation = Quaternion.Euler(
            baseEuler.x,
            UnityEngine.Random.Range(0f, 360f),
            baseEuler.z
        );

        Instantiate(cellStructure, centre, finalRotation, cell.transform);
    }

    // Instantiates a unit on the given cell
    private void CreateUnits(HexCell cell, Vector3 pos)
    {
        pos = chunkMesh.AddNoise(pos, 3);

        Vector3 centre = new Vector3(
            pos.x + this.transform.position.x,
            pos.y + this.transform.position.y,
            pos.z + this.transform.position.z
        );

        Unit prefab = MapManager.Instance.GetUnitPrefab();

        Vector3 baseEuler = prefab.transform.eulerAngles;

        Quaternion finalRotation = Quaternion.Euler(
            baseEuler.x,
            UnityEngine.Random.Range(0f, 360f),
            baseEuler.z
        );

        Unit unitInstance = Instantiate(prefab, centre, finalRotation,cell.transform);

        unitInstance.SetCurrentCell(cell);
        unitInstance.LayerIndex = level;

        TurnManager.Instance.RegisterUnit(unitInstance);
    }

    // Builds the entire chunk mesh
    public void RefreshChunk()
    {
        chunkMesh.Clear();

        chunkMesh.SetMeshData(level, transform.position, waterColorIndex, columnIndex == MapManager.Instance.xChunkCount-1);

        foreach (var cell in cells)
        {
            CreateCellMesh(cell);
        }

        chunkMesh.Apply();
    }
}
