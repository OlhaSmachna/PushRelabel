using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushRelabel
{
    public class Edge
    {
        public int flow, capacity;
        public int u, v;

        public Edge(int flow, int capacity, int u, int v)
        {
            this.flow = flow;
            this.capacity = capacity;
            this.u = u;
            this.v = v;
        }
    };

    public class Vertex
    {
        public int h, e_flow;

        public Vertex(int h, int e_flow)
        {
            this.h = h;
            this.e_flow = e_flow;
        }
    };

    public class Graph
    {
        int V;
        List<Vertex> v_list;
        public List<Edge> e_list;

        public Graph(int V)
        {
            this.V = V;
            v_list = new List<Vertex>();
            e_list = new List<Edge>();
            for (int i = 0; i < V; i++) v_list.Add(new Vertex(0, 0));
        }

        bool push(int u)
        {
            for (int i = 0; i < e_list.Count(); i++)
            {
                if (e_list[i].u == u)
                {
                    if (e_list[i].flow == e_list[i].capacity)continue;

                    if (v_list[u].h > v_list[e_list[i].v].h)
                    {
                        int flow = Math.Min(e_list[i].capacity - e_list[i].flow,
                            v_list[u].e_flow);
                        v_list[u].e_flow -= flow;
                        v_list[e_list[i].v].e_flow += flow;
                        e_list[i].flow += flow;
                        updateReverseEdgeFlow(i, flow);

                        return true;
                    }
                }
            }
            return false;
        }
        void relabel(int u)
        {
            int mh = 100000000;
            for (int i = 0; i < e_list.Count(); i++)
            {
                if (e_list[i].u == u)
                {
                    if (e_list[i].flow == e_list[i].capacity)
                        continue;
                    if (v_list[e_list[i].v].h < mh)
                    {
                        mh = v_list[e_list[i].v].h;
                        v_list[u].h = mh + 1;
                    }
                }
            }
        }


        void preflow(int s)
        {
            v_list[s].h = v_list.Count();

            for (int i = 0; i < e_list.Count(); i++)
            {
                if (e_list[i].u == s)
                {
                    e_list[i].flow = e_list[i].capacity;
                    v_list[e_list[i].v].e_flow += e_list[i].flow;
                    e_list.Add(new Edge(-e_list[i].flow, 0, e_list[i].v, s));
                }
            }
        }

        void updateReverseEdgeFlow(int i, int flow)
        {
            int u = e_list[i].v, v = e_list[i].u;

            for (int j = 0; j < e_list.Count(); j++)
            {
                if (e_list[j].v == v && e_list[j].u == u)
                {
                    e_list[j].flow -= flow;
                    return;
                }
            }

            Edge e = new Edge(0, flow, u, v);
            e_list.Add(e);
        }

        public void addEdge(int u, int v, int capacity)
        {
            e_list.Add(new Edge(0, capacity, u, v));
        }

        public int getMaxFlow(int s, int t)
        {
            preflow(s);
            while (overFlowVertex(v_list) != -1)
            {
                int u = overFlowVertex(v_list);
                if (!push(u))
                    relabel(u);
            }
            return v_list.Last().e_flow;
        }

        static int overFlowVertex(List<Vertex> ver)
        {
            for (int i = 1; i < ver.Count() - 1; i++)
                if (ver[i].e_flow > 0)
                    return i;
            return -1;
        }
    };

}
