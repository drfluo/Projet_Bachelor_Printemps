using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SimpleCity.AI
{
    public class AdjacencyGraph
    {
        Dictionary<Vertex, List<Vertex>> adjacencyDictionary = new Dictionary<Vertex, List<Vertex>>();

        public Vertex AddVertex(Vector3 position)
        {
            if(GetVertexAt(position) != null)
            {
                return null;
            }
            Vertex v = new Vertex(position);
            AddVertex(v);
            return v;

        }

        public void RemoveVertex(Vector3 position)
        {
            Vertex v = GetVertexAt(position);

            if(adjacencyDictionary.ContainsKey(v))
            {
                adjacencyDictionary.Remove(v);
            }
        }

        public bool isEmpty()
        {
            return adjacencyDictionary.Count() == 0;
        }

        private void AddVertex(Vertex v)
        {
            if (adjacencyDictionary.ContainsKey(v))
            {
                return;
            }
            adjacencyDictionary.Add(v, new List<Vertex>());
        }

        private Vertex GetVertexAt(Vector3 position)
        {
            return adjacencyDictionary.Keys.FirstOrDefault(x => CompareVertices(position, x.Position));
        }

        private bool CompareVertices(Vector3 position1, Vector3 position2)
        {
            return Vector3.SqrMagnitude(position1 - position2) < 0.0001f;
        }

        public void AddEdge(Vector3 position1, Vector3 position2)
        {
            if(CompareVertices(position1, position2))
            {
                return;
            }
            var v1 = GetVertexAt(position1);
            var v2 = GetVertexAt(position2);
            if(v1 == null)
            {
                v1 = new Vertex(position1);
                AddVertex(v1);
            }
            if(v2 == null)
            {
                v2 = new Vertex(position2);
                AddVertex(v2);
            }
            AddEdgeBetween(v1, v2);
            //AddEdgeBetween(v2, v1);

        }

        private void AddEdgeBetween(Vertex v1, Vertex v2)
        {
            if(v1 == v2)
            {
                return;
            }
            if (adjacencyDictionary.ContainsKey(v1))
            {
                if(adjacencyDictionary[v1].FirstOrDefault(x => x == v2) == null)
                {
                    adjacencyDictionary[v1].Add(v2);
                }
            }
            else
            {
                AddVertex(v1);
                adjacencyDictionary[v1].Add(v2);
            }

        }

        public List<Vertex> GetConnectedVerticesTo(Vertex v1)
        {
            if (adjacencyDictionary.ContainsKey(v1))
            {
                return adjacencyDictionary[v1];
            }
            List<Vertex> list = new List<Vertex>();
            return list;
        }

        public List<Vertex> GetConnectedVerticesTo(Vector3 position)
        {
            var v1 = GetVertexAt(position);
            if (v1 == null)
            {
                List<Vertex> list = new List<Vertex>();
                return list;
            
            }
            return adjacencyDictionary[v1];
        }

        public List<Vertex> GetVerticesConnectedTo(Vector3 position)
        {
            var v1 = GetVertexAt(position);
            List<Vertex> list = new List<Vertex>();

            if (v1 == null)
            {
                return list;
            }
            foreach (Vertex key in adjacencyDictionary.Keys)
            {
                if(adjacencyDictionary[key].Contains(v1))
                {
                    list.Add(key);
                }
            }
            return list;
        }

        public void ClearGraph()
        {
            adjacencyDictionary.Clear();
        }

        public IEnumerable<Vertex> GetVertices()
        {
            return adjacencyDictionary.Keys;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var vertex in adjacencyDictionary.Keys)
            {
                builder.AppendLine("Vertex " + vertex.ToString() + " neighbours: " + String.Join(", ", adjacencyDictionary[vertex]));
            }
            return builder.ToString();
        }

        public static List<Vector3> AStarSearch(AdjacencyGraph graph, Vector3 startPosition, Vector3 endPosition)
        {
            List<Vector3> path = new List<Vector3>();

            Vertex start = graph.GetVertexAt(startPosition);
            
            Vertex end = graph.GetVertexAt(endPosition);
            if (start == null || end==null)
            {
                Debug.Log("THERE IS A HUGE ISSUE WITH A VERTEX");
                return null;
            }

            List<Vertex> positionsTocheck = new List<Vertex>();
            Dictionary<Vertex, float> costDictionary = new Dictionary<Vertex, float>();
            Dictionary<Vertex, float> priorityDictionary = new Dictionary<Vertex, float>();
            Dictionary<Vertex, Vertex> parentsDictionary = new Dictionary<Vertex, Vertex>();

            positionsTocheck.Add(start);
            priorityDictionary.Add(start, 0);
            costDictionary.Add(start, 0);
            parentsDictionary.Add(start, null);

            while (positionsTocheck.Count > 0)
            {
                Vertex current = GetClosestVertex(positionsTocheck, priorityDictionary);
                positionsTocheck.Remove(current);
                if (current.Equals(end))
                {
                    path = GeneratePath(parentsDictionary, current);
                    return path;
                }

                foreach (Vertex neighbour in graph.GetConnectedVerticesTo(current))
                {
                    float newCost = costDictionary[current] + 1;
                    if (!costDictionary.ContainsKey(neighbour) || newCost < costDictionary[neighbour])
                    {
                        costDictionary[neighbour] = newCost;

                        float priority = newCost + ManhattanDiscance(end, neighbour);
                        positionsTocheck.Add(neighbour);
                        priorityDictionary[neighbour] = priority;

                        parentsDictionary[neighbour] = current;
                    }
                }
            }
            return path;
        }


        public void RemoveEdge(Vertex start, Vertex stop)
        {
            if (adjacencyDictionary.ContainsKey(start))
            {
                if (adjacencyDictionary[start].Contains(stop))
                {
                    adjacencyDictionary[start].Remove(stop);
                }
            }
        }

        public static List<Vector3> FindKthShortestPath(AdjacencyGraph graph, Vector3 startPosition, Vector3 endPosition, int K = 1)
        {
            List<Vector3> shortestPath= AStarSearch(graph, startPosition, endPosition);
            List<Vector3> secondPath= new List<Vector3>();
            List<Vector3> bestSecond = new List<Vector3>();
            int sizeSecond = int.MaxValue;

            if (K==1)
            {
                return shortestPath;
            }
            else if(shortestPath!=null)
            {
                for(int i=0;i<shortestPath.Count-2;i++)
                {
                    Vector3 removedEdgeStart = shortestPath[i];
                    Vector3 removedEdgeStop = shortestPath[i + 1];

                    // Remove edge from graph's adjacency dictionary
                    graph.RemoveEdge(graph.GetVertexAt(removedEdgeStart), graph.GetVertexAt(removedEdgeStop));
                    //recall A* if path found the return it 
                    secondPath = AStarSearch(graph, startPosition, endPosition);
                    graph.AddEdge(removedEdgeStart, removedEdgeStop);
                    if(secondPath.Count!=0 && secondPath.Count< sizeSecond)
                    {
                        bestSecond = secondPath;
                        sizeSecond = secondPath.Count;
                    }
                }
            }
            if(bestSecond.Count!=0)
            {
                return bestSecond;
            }

            return null;

        }


       /* public static List<Vector3> Yen(AdjacencyGraph graph, List<Vector3> shortestPath, int K)
        {
            List<Vector3>[] A = new List<Vector3>[K];
            A[0] = shortestPath;
            Debug.Log("First path size" + A[0].Count);

            List<Vector3> removedVertex = new List<Vector3>();
            List<Tuple<Vector3,Vector3>> removedEdge = new List<Tuple<Vector3,Vector3>>();

            Vector3 stop = shortestPath[shortestPath.Count - 1];

            List<List<Vector3>> B = new List<List<Vector3>>();

            for(int k=1;k<K;k++)
            {
                for(int i=0;i<=A[k-1].Count-2;i++)
                {
                    Vector3 spurNode = A[k - 1][i];
                    List<Vector3> rootpath = A[k-1].GetRange(0, i);

                    
                    foreach(List<Vector3> p in A)
                    {
                        if(p!=null)
                        {
                            if (rootpath == p.GetRange(0, i))
                            {
                                //delete p.edge i->i+1
                                graph.RemoveEdge(graph.GetVertexAt(p[i]), graph.GetVertexAt(p[i + 1]));
                                removedEdge.Add(new Tuple<Vector3, Vector3>(p[i], p[i + 1]));
                            }
                        }

                    }

                    foreach(Vector3 rootPathNode in rootpath)
                    {
                        if(rootPathNode!=spurNode)
                        {
                            //delete rootpathNode
                            foreach(Vertex connectedVertex in graph.GetConnectedVerticesTo(rootPathNode))
                            {
                                removedEdge.Add(new Tuple<Vector3, Vector3>(rootPathNode, connectedVertex.Position));
                            }

                            foreach(Vertex connectedVertex in graph.GetVerticesConnectedTo(rootPathNode))
                            {
                                removedEdge.Add(new Tuple<Vector3, Vector3>(connectedVertex.Position,rootPathNode));
                            }
                            
                            graph.RemoveVertex(rootPathNode);
                            removedVertex.Add(rootPathNode);
                        }
                    }

                    List<Vector3> spurPath= AStarSearch(graph, spurNode, stop);
                    if(spurPath!=null && spurPath.Count!=0)
                    {
                        rootpath.AddRange(spurPath);
                        List<Vector3> total = rootpath;

                        if (!B.Contains(total))
                        {
                            B.Add(total);
                        }
                    }
                    

                    //restore edge and nodes
                    foreach(Vector3 vertex in removedVertex)
                    {
                        graph.AddVertex(vertex);
                        
                    }
                    foreach(Tuple<Vector3,Vector3> edge in removedEdge)
                    {
                        graph.AddEdge(edge.Item1, edge.Item2);
                    }

                    removedEdge.Clear();
                    removedVertex.Clear();

                }

                if(B.Count==0)
                {
                    break;
                }
                B.Sort((a, b) => a.Count - b.Count);
                A[k] = B[0];
                B.RemoveAt(0);
            }

            for(int j=K-1;j>=0;j--)
            {
                if(A[j]!=null)
                {
                    Debug.Log(j);
                    return A[j];
                }
            }

            return null;
        }

        public static List<Vector3> Find3rdShortestPath(AdjacencyGraph graph, List<Vector3> shortestPath)
        {
            List<List<Vector3>> listAllPaths = new List<List<Vector3>> ();
            List<Tuple<Vector3,Vector3>> listRemoveEdges = new List<Tuple<Vector3, Vector3>>();
            List<Vector3> secondPath = new List<Vector3>();

            List<List<Vector3>> listSecondShortestPaths = new List<List<Vector3>>();

            for (int i = 0; i < shortestPath.Count - 2; i++)
            {
                Vector3 removedEdgeStart = shortestPath[i];
                Vector3 removedEdgeStop = shortestPath[i + 1];

                // Remove edge from graph's adjacency dictionary
                graph.RemoveEdge(graph.GetVertexAt(removedEdgeStart), graph.GetVertexAt(removedEdgeStop));
                //recall A* if path found the return it 
                secondPath = AStarSearch(graph, shortestPath[0], shortestPath[shortestPath.Count-1]);
                graph.AddEdge(removedEdgeStart, removedEdgeStop);
                if (secondPath.Count != 0)
                {
                    listSecondShortestPaths.Add(secondPath);
                    listRemoveEdges.Add(new Tuple<Vector3, Vector3>(removedEdgeStart, removedEdgeStop));
                    listAllPaths.Add(secondPath);
                }
            }

            int ipathId = 0;
            foreach(List<Vector3> secondShortest in listSecondShortestPaths)
            {
                //must remove edge that was removed to find this 2nd shortest otherwise find fastest again
                graph.RemoveEdge(graph.GetVertexAt(listRemoveEdges[ipathId].Item1), graph.GetVertexAt(listRemoveEdges[ipathId].Item2));
                
                for (int i = 0; i < secondShortest.Count - 2; i++)
                {
                    Vector3 removedEdgeStart = secondShortest[i];
                    Vector3 removedEdgeStop = secondShortest[i + 1];

                    // Remove edge from graph's adjacency dictionary
                    graph.RemoveEdge(graph.GetVertexAt(removedEdgeStart), graph.GetVertexAt(removedEdgeStop));
                    //recall A* if path found the return it 
                    secondPath = AStarSearch(graph, secondShortest[0], secondShortest[secondShortest.Count - 1]);
                    graph.AddEdge(removedEdgeStart, removedEdgeStop);
                    if (secondPath.Count != 0)
                    {
                        listAllPaths.Add(secondPath);
                    }
                }
                graph.AddEdge(listRemoveEdges[ipathId].Item1, listRemoveEdges[ipathId].Item2);
                ipathId++;
            }
            //from the list of all possible paths, we must take the second (the best is not included so the 2nd of the list is third best)

            if(listAllPaths.Count<2)
            {
                return null;
            }

            List<Vector3> first = listAllPaths[0];
            
            foreach(List<Vector3> pathNext in listAllPaths)
            {
                if (first.Count > pathNext.Count)
                {
                    Debug.Log(first.Count);
                    first = pathNext;
                }
            }


            // Find second
            // largest element
            List<Vector3> second =new List<Vector3>();
            int iSize = int.MaxValue;
            foreach (List<Vector3> pathNext in listAllPaths)
            {
                if (iSize > pathNext.Count && first!=pathNext)
                {
                    Debug.Log("Second replaced");
                    second = pathNext;
                    iSize = pathNext.Count;
                }
            }
            
            if(second.Count!=0)
            {
                Debug.Log("Second of size "+second.Count);
                return second;
            }
            else
            {
                Debug.Log("COUNT WAS 0");
                if(first.Count!=0)
                {
                    return first;
                }
                else
                {
                    Debug.Log("COUNT WAS DOUBLY 0");
                    return shortestPath;
                }
            }
        }*/



        private static Vertex GetClosestVertex(List<Vertex> list, Dictionary<Vertex, float> distanceMap)
        {
            Vertex candidate = list[0];
            foreach (Vertex vertex in list)
            {
                if (distanceMap[vertex] < distanceMap[candidate])
                {
                    candidate = vertex;
                }
            }
            return candidate;
        }

        private static float ManhattanDiscance(Vertex endPos, Vertex position)
        {
            return Math.Abs(endPos.Position.x - position.Position.x) + Math.Abs(endPos.Position.z - position.Position.z);
        }

        public static List<Vector3> GeneratePath(Dictionary<Vertex, Vertex> parentMap, Vertex endState)
        {
            List<Vector3> path = new List<Vector3>();
            Vertex parent = endState;
            while (parent != null && parentMap.ContainsKey(parent))
            {
                path.Add(parent.Position);
                parent = parentMap[parent];
            }
            path.Reverse();
            return path;
        }

        public static List<Vertex> GeneratePathVertex(Dictionary<Vertex, Vertex> parentMap, Vertex endState)
        {
            List<Vertex> path = new List<Vertex>();
            Vertex parent = endState;
            while (parent != null && parentMap.ContainsKey(parent))
            {
                path.Add(parent);
                parent = parentMap[parent];
            }
            path.Reverse();
            return path;
        }
    }
}

