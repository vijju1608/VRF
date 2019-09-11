using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model
{
    public class Nodes<T>  : ModelBase
    {
        T data;
        Nodes<T> Lnode, Rnode, Pnode;

        public T Data  //中  
        {
            set { this.SetValue(ref data, value); }
            get { return data; }
        }

        public Nodes<T> LNode //左  
        {
            get { return Lnode; }
            set { this.SetValue(ref Lnode, value); }
        }
        public Nodes<T> RNode //右  
        {
            set { this.SetValue(ref Rnode, value); }
            get { return Rnode; }
        }

        public Nodes<T> PNode
        {
            set { this.SetValue(ref Pnode, value); }
            get { return Pnode; }
        }
        public Nodes() { }

        public Nodes(T data)
        {
            this.data = data;
        }

        //先序遍历  
        public static void PreOrder<T>(Nodes<T> rootNode)
        {
            if (rootNode != null)
            {
                Console.WriteLine(rootNode.Data);
                PreOrder<T>(rootNode.LNode);
                PreOrder<T>(rootNode.RNode);
            }
        }
        //中序遍历二叉树  
        public static void MidOrder<T>(Nodes<T> rootNode)
        {
            if (rootNode != null)
            {
                MidOrder<T>(rootNode.LNode);
                Console.WriteLine(rootNode.Data);
                MidOrder<T>(rootNode.RNode);
            }
        }

        //后续遍历二叉树  
        public static void AfterOrder<T>(Nodes<T> rootNode)
        {
            if (rootNode != null)
            {
                AfterOrder<T>(rootNode.LNode);
                AfterOrder<T>(rootNode.RNode);
                Console.WriteLine(rootNode.Data);
            }
        }
        //层次遍历  
        public static void LayerOrder<T>(Nodes<T> rootNode)
        {
            Nodes<T>[] Nodes = new Nodes<T>[20];
            int front = -1; //前  
            int rear = -1;  //后  
            if (rootNode != null)
            {
                rear++;
                Nodes[rear] = rootNode;
            }
            while (front != rear)
            {
                front++;
                rootNode = Nodes[front];
                Console.WriteLine(rootNode.Data);
                if (rootNode.LNode != null)
                {
                    rear++;
                    Nodes[rear] = rootNode.LNode;
                }
                if (rootNode.RNode != null)
                {
                    rear++;
                    Nodes[rear] = rootNode.RNode;
                }
            }
        }

    }
}
