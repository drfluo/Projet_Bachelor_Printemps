using System;
using System.Collections.Generic;
using UnityEngine;

//A point is two int (x and y)
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj is Point)
        {
            Point p = obj as Point;
            return this.X == p.X && this.Y == p.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 6949;
            hash = hash * 7907 + X.GetHashCode();
            hash = hash * 7907 + Y.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "P(" + this.X + ", " + this.Y + ")";
    }
}

public enum CellType
{
    Empty,
    Road,
    Structure,
    SpecialStructure,
    None
}


//A grid is a set of 3 lists of points (road, house and special)
public class Grid
{
    private CellType[,] _grid;
    private int _width;
    public int Width { get { return _width; } }
    private int _height;
    public int Height { get { return _height; } }

    private List<Point> _roadList = new List<Point>();
    private List<Point> _specialStructure = new List<Point>();
    private List<Point> _houseStructure = new List<Point>();

    public Grid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new CellType[width, height];
    }

    // Adding index operator to our Grid class so that we can use grid[][] to access specific cell from our grid. 
    public CellType this[int i, int j]
    {
        get
        {
            return _grid[i, j];
        }
        set
        {
            if (value == CellType.Road)
            {
                _roadList.Add(new Point(i, j));
            }
            if (value == CellType.SpecialStructure)
            {
                _specialStructure.Add(new Point(i, j));
            }
            if (value == CellType.Structure)
            {
                _houseStructure.Add(new Point(i, j));
            }
            if(value==CellType.Empty)
            {
                CellType type = _grid[i, j];
                if(type==CellType.Road)
                {
                    _roadList.Remove(new Point(i, j));
                }
                else if (type == CellType.SpecialStructure)
                {
                    _specialStructure.Remove(new Point(i, j));
                }
                else if (type == CellType.Structure)
                {
                    _houseStructure.Remove(new Point(i, j));
                }
            }
            _grid[i, j] = value;
        }
    }

    public static bool IsCellWakable(CellType cellType, bool aiAgent = false)
    {
        if (aiAgent) //if we are a car a cell is interesting if it is a road
        {
            return cellType == CellType.Road;
        }
        //If we are placing a road the "interesting" cells are empty and road
        return cellType == CellType.Empty || cellType == CellType.Road;
    }

    public Point GetRandomRoadPoint()
    {
        if (_roadList.Count == 0)
        {
            return null;
        }
        return _roadList[UnityEngine.Random.Range(0, _roadList.Count)];
    }

    public Point GetRandomSpecialStructurePoint()
    {
        if (_specialStructure.Count == 0)
        {
            return null;
        }
        return _specialStructure[UnityEngine.Random.Range(0, _specialStructure.Count)];
    }

    public Point GetRandomHouseStructurePoint()
    {
        if (_houseStructure.Count == 0)
        {
            return null;
        }
        return _houseStructure[UnityEngine.Random.Range(0, _houseStructure.Count)];
    }

    public List<Point> GetAllHouses()
    {
        return _houseStructure;
    }

    internal List<Point> GetAllSpecialStructure()
    {
        return _specialStructure;
    }

    public List<Point> GetAllRoads()
    {
        return _roadList;
    }

    public List<Point> GetAdjacentCells(Point cell, bool isAgent)
    {
        List<Point> adjacentCells = GetAllAdjacentCells((int)cell.X, (int)cell.Y);
        for (int i = adjacentCells.Count - 1; i >= 0; i--)
        {
            if (IsCellWakable(_grid[adjacentCells[i].X, adjacentCells[i].Y], isAgent) == false)
            {
                adjacentCells.RemoveAt(i);
            }
        }
        return adjacentCells;
    }

    public float GetCostOfEnteringCell(Point cell)
    {
        return 1;
    }

    public List<Point> GetAllAdjacentCells(int x, int y)
    {
        List<Point> adjacentCells = new List<Point>();
        if (x > 0)
        {
            adjacentCells.Add(new Point(x - 1, y));
        }
        if (x < _width - 1)
        {
            adjacentCells.Add(new Point(x + 1, y));
        }
        if (y > 0)
        {
            adjacentCells.Add(new Point(x, y - 1));
        }
        if (y < _height - 1)
        {
            adjacentCells.Add(new Point(x, y + 1));
        }
        return adjacentCells;
    }


    public List<Point> GetAdjacentCellsOfType(int x, int y, CellType type)
    {
        List<Point> adjacentCells = GetAllAdjacentCells(x, y);
        for (int i = adjacentCells.Count - 1; i >= 0; i--)
        {
            if (_grid[adjacentCells[i].X, adjacentCells[i].Y] != type)
            {
                adjacentCells.RemoveAt(i);
            }
        }
        return adjacentCells;
    }

  
    public CellType[] GetAllAdjacentCellTypes(int x, int y)
    {
        CellType[] neighbours = { CellType.None, CellType.None, CellType.None, CellType.None };
        if (x > 0)
        {
            neighbours[0] = _grid[x - 1, y];
        }
        if (x < _width - 1)
        {
            neighbours[2] = _grid[x + 1, y];
        }
        if (y > 0)
        {
            neighbours[3] = _grid[x, y - 1];
        }
        if (y < _height - 1)
        {
            neighbours[1] = _grid[x, y + 1];
        }
        return neighbours;
    }

    internal void removeElementFromList(Vector3Int position)
    {
        Point p = new Point(position.x, position.z);
        foreach (var point in _houseStructure)
        {
            if(point.Equals(p))
            {
                _houseStructure.Remove(point);
                return;
            }
        }
        foreach (var point in _specialStructure)
        {
            if (point.Equals(p))
            {
                _specialStructure.Remove(point);
                return;
            }
        }
    }
}
