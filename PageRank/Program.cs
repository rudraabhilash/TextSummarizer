using System;
using TextSummarizer.Graph;
using TextSummarizer.Rank;

namespace PageRank
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DirectedGraph<string> G = new DirectedGraph<string>();

            G.AddEdge("A", "B", 1);
            G.AddEdge("C", "D", 1);
            G.AddEdge("G", "D", 1);
            G.AddEdge("D", "A", 2);
            G.AddEdge("D", "E", 2);
            G.AddEdge("B", "D", 2);
            G.AddEdge("D", "E", 2);
            G.AddEdge("B", "C", 3);
            G.AddEdge("E", "F", 3);
            G.AddEdge("C", "F", 4);

            //web.Display();
            var a = G.OutDegree;
            foreach (var item in a)
            {
                GraphNode<String> key = item.Key;
                Console.WriteLine(key.Value + "  " + item.Value);
            }

            var n = G.Nodes;
            foreach (var item in n)
            {
                Console.WriteLine(item.Value);
            }

            TextRank<string> p = new TextRank<string>();
            p.Rank<string>(G);
        }
    }
}