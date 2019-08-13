using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class collisionMap
{
    public Vector3Int mapSize = new Vector3Int( 0, 0, 0 ); // bottom left is (0,0) top right (x,y)
    public Vector3Int bottomLeftPoint = new Vector3Int( 0, 0, 0 );
    bool[,] boolMap; // true means can walk there

    public void initMap()
    {
        boolMap = new bool[ mapSize.x, mapSize.y ];
        for (int i = 0; i < mapSize.x; i++ )
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                boolMap[i, j] = false;
            }
        }
    }

    public void setBoolMapValue( Vector3Int position, bool value )
    {
        if( isValidMapPosition(position) )
        {
            boolMap[position.x, position.y] = value;
        }
    }

    public bool getBoolMapValue( Vector3Int position )
    {
        if (isValidMapPosition(position))
        {
            return boolMap[position.x,position.y];
        }
        return false;
    }

    public bool isValidMapPosition( Vector3Int position )
    {
        if( position.x >=0 && position.y >=0 && position.x < mapSize.x && position.y < mapSize.y )
        {
            return true;
        }
        return false;
    }
}

public class pathfinding : MonoBehaviour {
    
    public Grid mainGrid;
    public Tilemap ground;
    public Tilemap top_ground;
    public Tilemap planting;
    public Tilemap borders;
    public Tilemap water;
    public collisionMap mainCollisionMap = new collisionMap();

    public int maxSearch = 150;

	// Use this for initialization
	void Start()
    {
        setMapSize();
        setBottomLeftCorner();
        buildCollisionMap();
    }

    void setBottomLeftCorner()
    {
        mainCollisionMap.bottomLeftPoint = ground.origin; // gets bottom left most point

        mainCollisionMap.bottomLeftPoint = Vector3Int.Min(mainCollisionMap.bottomLeftPoint,top_ground.origin); 
        mainCollisionMap.bottomLeftPoint = Vector3Int.Min(mainCollisionMap.bottomLeftPoint,planting.origin); 
        mainCollisionMap.bottomLeftPoint = Vector3Int.Min(mainCollisionMap.bottomLeftPoint,borders.origin); 
        mainCollisionMap.bottomLeftPoint = Vector3Int.Min(mainCollisionMap.bottomLeftPoint,water.origin); 
    }

    void setMapSize()
    {              
        mainCollisionMap.mapSize.x = Mathf.Max(mainCollisionMap.mapSize.x, ground.size.x);
        mainCollisionMap.mapSize.y = Mathf.Max(mainCollisionMap.mapSize.y, ground.size.y);

        mainCollisionMap.mapSize.x = Mathf.Max(mainCollisionMap.mapSize.x, top_ground.size.x);
        mainCollisionMap.mapSize.y = Mathf.Max(mainCollisionMap.mapSize.y, top_ground.size.y);

        mainCollisionMap.mapSize.x = Mathf.Max(mainCollisionMap.mapSize.x, planting.size.x);
        mainCollisionMap.mapSize.y = Mathf.Max(mainCollisionMap.mapSize.y, planting.size.y);

        mainCollisionMap.mapSize.x = Mathf.Max(mainCollisionMap.mapSize.x, borders.size.x);
        mainCollisionMap.mapSize.y = Mathf.Max(mainCollisionMap.mapSize.y, borders.size.y);

        mainCollisionMap.mapSize.x = Mathf.Max(mainCollisionMap.mapSize.x, water.size.x);
        mainCollisionMap.mapSize.y = Mathf.Max(mainCollisionMap.mapSize.y, water.size.y);
    }

    void buildCollisionMap()
    {
        mainCollisionMap.initMap();

        for( int i = 0; i < mainCollisionMap.mapSize.x; i++ )
        {
            for( int j = 0; j < mainCollisionMap.mapSize.y; j++ )
            {
                Vector3Int cPosition = new Vector3Int(i, j, 0);
                mainCollisionMap.setBoolMapValue( cPosition, isWalkableSpace(convertBoolPointToCellPoint(cPosition)) );
            }
        }
    }

    public Vector3Int convertCellPointToBoolPoint( Vector3Int point )
    {
        return point - mainCollisionMap.bottomLeftPoint;
    }

    public Vector3Int convertBoolPointToCellPoint( Vector3Int point )
    {
        return point + mainCollisionMap.bottomLeftPoint;
    }

    bool isWalkableSpace( Vector3Int position )
    {
        bool rValue = true;

        TileBase cTile = ground.GetTile(position);

        if( cTile && !isTileTypeWalkable( cTile.name ) )
        {
            if( rValue )
            {
                rValue = false;
            }
        }

        cTile = top_ground.GetTile(position);
        if (cTile && !isTileTypeWalkable( cTile.name ) )
        {
            if (rValue)
            {
                rValue = false;
            }
        }

        cTile = planting.GetTile(position);
        if (cTile && !isTileTypeWalkable( cTile.name ) )
        {
            if (rValue)
            {
                rValue = false;
            }
        }

        cTile = borders.GetTile(position);
        if (cTile && !isTileTypeWalkable( cTile.name ) )
        {
            if (rValue)
            {
                rValue = false;
            }
        }

        cTile = water.GetTile(position);
        if (cTile && !isTileTypeWalkable( cTile.name ) )
        {
            if (rValue)
            {
                rValue = false;
            }
        }

        return rValue;
    }

    public List<Vector3> getMoveDirections( Vector3 startPosition, Vector3 goalPosition )
    {
        Vector3Int intStartPosition = mainGrid.WorldToCell(startPosition);
        Vector3Int intGoalPosition = mainGrid.WorldToCell(goalPosition);

        List<Vector3Int> relativePath = findPath(intStartPosition, intGoalPosition);
        List<Vector3> worldPath = new List<Vector3>();

        foreach( Vector3Int c in relativePath)
        {
            Vector3 newVec = mainGrid.CellToWorld(convertBoolPointToCellPoint(c));
            newVec.x += mainGrid.cellSize.x / 2f;
            newVec.y += mainGrid.cellSize.y / 2f;
            worldPath.Add(newVec);

            // worldPath.Add(mainGrid.CellToWorld(convertBoolPointToCellPoint(c)));
        }

        return worldPath;
    }

    // https://en.wikipedia.org/wiki/A*_search_algorithm
    List<Vector3Int> findPath( Vector3Int startPosition, Vector3Int goalPosition )
    {
        Vector3Int relativeStart = convertCellPointToBoolPoint(startPosition);
        Vector3Int relativeGoal = convertCellPointToBoolPoint(goalPosition);

        if(!mainCollisionMap.getBoolMapValue(relativeGoal))
        {
            return new List<Vector3Int>();
        }

        if( !mainCollisionMap.isValidMapPosition(relativeStart) ||
           !mainCollisionMap.isValidMapPosition(relativeGoal) )
        {
            return new List<Vector3Int>(); // if start and end are not valid, then no path exists
        }

        List<Vector3Int> closedSet = new List<Vector3Int>();

        List<Vector3Int> openSet = new List<Vector3Int>();
        openSet.Add(relativeStart);

        Vector3Int mSize = mainCollisionMap.mapSize;

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        float[,] gScore = new float[mSize.x, mSize.y];
        float[,] fScore = new float[mSize.x, mSize.y];

        float infinityValue = float.MaxValue / 2f;

        for ( int i = 0; i < mSize.x; i++ )
        {
            for (int j = 0; j < mSize.y; j++ )
            {
                gScore[i, j] = infinityValue;
                fScore[i, j] = infinityValue;
            }
        }

        gScore[relativeStart.x, relativeStart.y] = 0f;
        fScore[relativeStart.x, relativeStart.y] = heuristic_cost_estimate(relativeStart, relativeGoal);

        int numSearched = 0;

        while (openSet.Count > 0)
        {
            Vector3Int current = findLowestFScore( ref openSet, ref fScore );

            // check if current is goal
            if( current == relativeGoal )
            {
                return reconstructPath( ref cameFrom, current );
            }

            numSearched++;
            if( numSearched == maxSearch )
            {
                return new List<Vector3Int>();
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // add all uindiscovered nodes
            List<Vector3Int> neighbors = new List<Vector3Int>();
            getAllWalkableNeighbors(current, ref neighbors);

            foreach( Vector3Int cNeighbor in neighbors )
            {
                if( closedSet.Contains(cNeighbor) )
                {
                    continue; // ignore evaluated node
                }

                if( !openSet.Contains(cNeighbor))
                {
                    openSet.Add(cNeighbor); // add new node
                }

                float tentativegScore = gScore[current.x, current.y] + distanceBetweenPoints(current,cNeighbor);
                if( tentativegScore >= gScore[cNeighbor.x, cNeighbor.y] )
                {
                    continue; // not fastest path
                }

                // found most efficient path (for now)
                cameFrom[cNeighbor] = current;
                gScore[cNeighbor.x,cNeighbor.y] = tentativegScore;
                fScore[cNeighbor.x,cNeighbor.y] = gScore[cNeighbor.x, cNeighbor.y] + heuristic_cost_estimate(cNeighbor, relativeGoal);
            }
        }
        return new List<Vector3Int>();
    }

    float distanceBetweenPoints( Vector3Int point1, Vector3Int point2 )
    {
        float xDist = point2.x - point1.x;
        float yDist = point2.y - point1.y;
        // return xDist * xDist + yDist * yDist;
        return Mathf.Sqrt(xDist*xDist + yDist*yDist);
    }

    void getAllNeighbors( Vector3Int position, ref List<Vector3Int> neighbors )
    {
    }

    void getAllWalkableNeighbors( Vector3Int position, ref List<Vector3Int> neighbors )
    {
        // convertWorldPointToBoolPoint
        Vector3Int topPosition = position;
        topPosition.y += 1;
        Vector3Int rightPosition = position;
        rightPosition.x += 1;
        Vector3Int bottomPosition = position;
        bottomPosition.y -= 1;
        Vector3Int leftPosition = position;
        leftPosition.x -= 1;

        Vector3Int topRightPosition = position;
        topRightPosition.x += 1;
        topRightPosition.y += 1;
        Vector3Int bottomRightPosition = position;
        bottomRightPosition.x += 1;
        bottomRightPosition.y -= 1;
        Vector3Int bottomLeftPosition = position;
        bottomLeftPosition.x -= 1;
        bottomLeftPosition.y -= 1;
        Vector3Int topLeftPosition = position;
        topLeftPosition.x -= 1;
        topLeftPosition.y += 1;

        bool addedTop = false;
        bool addedRight = false;
        bool addedBottom = false;
        bool addedLeft = false;

        if( mainCollisionMap.getBoolMapValue(topPosition))
        {
            neighbors.Add(topPosition);
            addedTop = true;
        }

        if (mainCollisionMap.getBoolMapValue(rightPosition))
        {
            neighbors.Add(rightPosition);
            addedRight = true;
        }

        if (mainCollisionMap.getBoolMapValue(bottomPosition))
        {
            neighbors.Add(bottomPosition);
            addedBottom = true;
        }

        if (mainCollisionMap.getBoolMapValue(leftPosition))
        {
            neighbors.Add(leftPosition);
            addedLeft = true;
        }

        if( addedTop && addedRight )
        {
            if (mainCollisionMap.getBoolMapValue(topRightPosition))
            {
                neighbors.Add(topRightPosition);
            }
        }

        if ( addedRight && addedBottom )
        {
            if (mainCollisionMap.getBoolMapValue(bottomRightPosition))
            {
                neighbors.Add(bottomRightPosition);
            }
        }

        if (addedBottom && addedLeft)
        {
            if (mainCollisionMap.getBoolMapValue(bottomLeftPosition))
            {
                neighbors.Add(bottomLeftPosition);
            }
        }

        if (addedTop && addedLeft)
        {
            if (mainCollisionMap.getBoolMapValue(topLeftPosition))
            {
                neighbors.Add(topLeftPosition);
            }
        }
    }

    Vector3Int findLowestFScore( ref List<Vector3Int> openSet, ref float[,] fScore )
    {
        Vector3Int currentLowestVec = openSet[0];
        float currentLowestValue = fScore[currentLowestVec.x,currentLowestVec.y];
        for( int i = 1; i < openSet.Count; i++ )// Vector3Int current in openSet )
        {
            Vector3Int current = openSet[i];

            if( fScore[current.x,current.y] < currentLowestValue )
            {
                currentLowestVec = current;
                currentLowestValue = fScore[current.x, current.y];
            }
        }
        return currentLowestVec;
    }

    float heuristic_cost_estimate(Vector3Int current, Vector3Int goal) // start, goal
    {
        float D = 1f;
        float DD = 1.414214f;
        float dx = Mathf.Abs(current.x - goal.x);
        float dy = Mathf.Abs(current.y - goal.y);
        return D * (dx + dy) + (DD - 2f * D) * Mathf.Min(dx, dy);
    }

    List<Vector3Int> reconstructPath( ref Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current )
    {
        List<Vector3Int> totalPath = new List<Vector3Int>();
        totalPath.Add(current);
        bool exists = true;
        while(exists)
        {
            if( cameFrom.ContainsKey(current) )
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            else
            {
                break;
            }
        }
        return totalPath;
    }

    bool isTileTypeWalkable( string tileName )
    {
        bool rValue = true;
        switch( tileName )
        {
            case "drieddirt":
                rValue = true;
                break;
            case "water":
                rValue = false;
                break;
            case "water3":
                rValue = false;
                break;
            case "mediumRock":
                rValue = false;
                break;
            case "tree":
                rValue = false;
                break;
            case "reedLeft":
                rValue = false;
                break;
            case "reedMiddle":
                rValue = false;
                break;
            case "reedRight":
                rValue = false;
                break;
            case "centerUpDown":
                rValue = false;
                break;
            case "centerDown":
                rValue = false;
                break;
            default:
                rValue = false;
                break;
        }
        return rValue;
    }
}
