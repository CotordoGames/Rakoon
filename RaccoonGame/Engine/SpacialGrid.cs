using Raylib_cs;

public sealed class SpacialGrid
{
    readonly float cell; //width and height of 1 square cell in world pixels
    
    //keys are the column and row of a cell, and the value is all the objects touching that cell
    readonly Dictionary<(int, int), List<GameObject>> cells = new Dictionary<(int, int), List<GameObject>>();
    
    public SpacialGrid(float cellSize) {
        cell = cellSize;
    }
    
    //world coordinates to cell index
    int Cell(float worldCoord) => (int)MathF.Floor(worldCoord / cell);

    public void Clear()
    {
        foreach (var list in cells.Values)
        {
            list.Clear();
        }
    }

    //add the object to all of the cells that it overlaps' lists
    public void Insert(GameObject o)
    {
        Rectangle r = o.BoundingBox;
        int x0 = Cell(r.X), x1 = Cell(r.X + r.Width);
        int y0 = Cell(r.Y),  y1 = Cell(r.Y + r.Height);

        for (int cy = y0; cy <= y1; cy++)
        {
            for (int cx = x0; cx <= x1; cx++)
            {
                if (!cells.TryGetValue((cx, cy), out List<GameObject> list))
                    cells[(cx, cy)] = list = new List<GameObject>();
                
                list.Add(o);
            }
        }
    }
    
    //get all of the objects in the cells overlapping the Rectangle r
    public void Query(Rectangle r, List<GameObject> results)
    {
        int x0 = Cell(r.X), x1 = Cell(r.X + r.Width);
        int y0 = Cell(r.Y),  y1 = Cell(r.Y + r.Height);

        for (int cy = y0; cy <= y1; cy++)
        {
            for (int cx = x0; cx <= x1; cx++)
            {
                if(cells.TryGetValue((cx, cy), out List<GameObject> list)) results.AddRange(list);
            }
        }
    }
}