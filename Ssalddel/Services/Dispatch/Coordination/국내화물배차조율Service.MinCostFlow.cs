namespace 살뜰.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율Service
{
    private sealed class MinCostFlowGraph
    {
        private readonly List<Edge>[] _graph;
        private readonly List<Edge> _assignmentEdges = [];

        public MinCostFlowGraph(int nodeCount)
        {
            _graph = Enumerable.Range(0, nodeCount).Select(_ => new List<Edge>()).ToArray();
        }

        public void AddEdge(int from, int to, int capacity, long cost, 운송의뢰기사조합평가? candidate)
        {
            var forward = new Edge(to, _graph[to].Count, capacity, cost, candidate);
            var reverse = new Edge(from, _graph[from].Count, 0, -cost, null);
            _graph[from].Add(forward);
            _graph[to].Add(reverse);

            if (candidate is not null)
            {
                _assignmentEdges.Add(forward);
            }
        }

        public void Run(int source, int sink, int maxFlow)
        {
            for (var flow = 0; flow < maxFlow;)
            {
                var previousNode = new int[_graph.Length];
                var previousEdge = new int[_graph.Length];
                var distance = Enumerable.Repeat(long.MaxValue, _graph.Length).ToArray();
                var inQueue = new bool[_graph.Length];
                var queue = new Queue<int>();

                distance[source] = 0;
                queue.Enqueue(source);
                inQueue[source] = true;

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    inQueue[node] = false;

                    for (var i = 0; i < _graph[node].Count; i++)
                    {
                        var edge = _graph[node][i];
                        if (edge.Capacity <= 0 || distance[node] == long.MaxValue)
                        {
                            continue;
                        }

                        var nextDistance = distance[node] + edge.Cost;
                        if (nextDistance >= distance[edge.To])
                        {
                            continue;
                        }

                        distance[edge.To] = nextDistance;
                        previousNode[edge.To] = node;
                        previousEdge[edge.To] = i;
                        if (!inQueue[edge.To])
                        {
                            queue.Enqueue(edge.To);
                            inQueue[edge.To] = true;
                        }
                    }
                }

                if (distance[sink] == long.MaxValue)
                {
                    break;
                }

                var addFlow = maxFlow - flow;
                for (var node = sink; node != source; node = previousNode[node])
                {
                    addFlow = Math.Min(addFlow, _graph[previousNode[node]][previousEdge[node]].Capacity);
                }

                for (var node = sink; node != source; node = previousNode[node])
                {
                    var edge = _graph[previousNode[node]][previousEdge[node]];
                    edge.Capacity -= addFlow;
                    _graph[node][edge.Reverse].Capacity += addFlow;
                }

                flow += addFlow;
            }
        }

        public IReadOnlyList<운송의뢰기사조합평가> AssignedCandidates()
            => _assignmentEdges
                .Where(x => x.Candidate is not null && x.Capacity == 0)
                .Select(x => x.Candidate!)
                .ToArray();

        private sealed class Edge
        {
            public Edge(int to, int reverse, int capacity, long cost, 운송의뢰기사조합평가? candidate)
            {
                To = to;
                Reverse = reverse;
                Capacity = capacity;
                Cost = cost;
                Candidate = candidate;
            }

            public int To { get; }

            public int Reverse { get; }

            public int Capacity { get; set; }

            public long Cost { get; }

            public 운송의뢰기사조합평가? Candidate { get; }
        }
    }
}
