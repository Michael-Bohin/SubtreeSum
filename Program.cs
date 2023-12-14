using System.ComponentModel.Design;
using System.Security.Cryptography;
using static System.Console;

WriteLine("Hello, Subtree World!\n");

List<Vertex> graph = new() {
	new("A", new() { "B", "C"} ),
	new("B", new() { "D", "E", "F"} ),
	new("C", new() { "D", "F"} ),
	new("D", new() { "G", "H"} ),
	new("E", new() { "H", "I"} ),
	new("F", new() { "I"} ),
	new("G", new() { "J", "K"} ),
	new("H", new() { "K"} ),
	new("I", new() { "K"} ),
	new("J", new() { } ),
	new("K", new() { } ) 
};

WriteLine("    >>> FCP <<<");
FirstCommonParent fcp = new(graph);
fcp.CalculateSubtreeSums();
fcp.PrintResult();


// reset graph instances just to be sure:
graph = new() {
	new("A", new() { "B", "C"} ),
	new("B", new() { "D", "E", "F"} ),
	new("C", new() { "D", "F"} ),
	new("D", new() { "G", "H"} ),
	new("E", new() { "H", "I"} ),
	new("F", new() { "I"} ),
	new("G", new() { "J", "K"} ),
	new("H", new() { "K"} ),
	new("I", new() { "K"} ),
	new("J", new() { } ),
	new("K", new() { } )
};

WriteLine("\n\n    >>> SPCM <<<");
SubtractPathCountMultiples spcm = new(graph);
spcm.CalculateSubtreeSums();
spcm.PrintResult();

enum Color { White, Grey, Black };

class Vertex {
	public readonly string Name;
	public readonly int TimeInSeconds = 1;
	public readonly List<string> StringChildren;
	public readonly List<Vertex> Children = new();

	// values modified dynamically during crawl:
	public int SubtreeSum = 0;
	public Vertex? LastBlackParent = null;
	public Color Color = Color.White;
	public int PathsFromSourceCount = 0;

	public Vertex(string name, List<string> children) {
		Name = name;
		StringChildren = children;
	}
}

abstract class SubtreeCrawler {
	protected Dictionary<string, Vertex> graph = new();

	public SubtreeCrawler(List<Vertex> vertices) { 
		foreach(Vertex v in vertices)
			graph[v.Name] = v;

		foreach(Vertex v in graph.Values) 
			foreach(string child in v.StringChildren) 
				v.Children.Add(graph[child]);
	}

	public void CalculateSubtreeSums() {
		Subtree(graph["A"]);
	}

	protected abstract void Subtree(Vertex v);

	public void PrintResult() {
		WriteLine("Subtree sums:");
		foreach(Vertex v in graph.Values)
			WriteLine($"{v.Name}: {v.SubtreeSum}");
	}
}

// DFS - substract first common predecessor approach
// 'FCP'

class FirstCommonParent : SubtreeCrawler { 
	public FirstCommonParent(List<Vertex> g) : base(g) { }

	protected override void Subtree(Vertex v){
		v.Color = Color.Grey;
		WriteLine(v.Name);

		foreach(Vertex child in v.Children) {
			if(child.Color == Color.Grey)
				throw new InvalidOperationException();		

			if (child.Color == Color.White) {
				Subtree(child);
			} else if (child.Color == Color.Black) {
				Vertex firstCommonPredecessor = FindFirstCommonPredecessor(child);
				firstCommonPredecessor.SubtreeSum -= child.SubtreeSum;
			}
			v.SubtreeSum += child.SubtreeSum;
			// this repointing of backward paths must happend AFTER call of FFCP procedure of black child
			child.LastBlackParent = v; // this happens regardless we return from initialy white, or if we just observe black and dont actually call self there
		}

		if (v.Name == "H")
			WriteLine("Debug will start here");

		// add value of time ins of self..:
		v.SubtreeSum++;
		v.Color = Color.Black;
		// return v.SubtreeSum;
	}

	private Vertex FindFirstCommonPredecessor(Vertex from) {
		Vertex temp = from.LastBlackParent!;
		while (temp!.Color == Color.Black)
			temp = temp.LastBlackParent!;
		return temp;
	}
}

// DFS sum followed by DFS path counts to each vertex -> substract multiples - 1 of path count * vertex value foreach vertex
// 'SPCM'
// V neosetrenem DFS se duplicity od nejakeho vrcholu v opakuji prave tolikrat,
// kolik ruznych cest vede do vrcholu v minus 1. Staci tady odecist vahu vrcholu krat (path count from root -1) 
// !! Pro obecny case s vice zdroji nad top sort je nejspis treba osetrit umelym superzdrojem (na promysleni) 
class SubtractPathCountMultiples : SubtreeCrawler {
	public SubtractPathCountMultiples(List<Vertex> g): base(g) { }

	// Gameplan:
	// 1. sum = Run dumm DFS that retrieves sum with duplicities
	// 2. Run DFS that counts count of different paths to each Vertex
	// 3. Foreach vertex v 
	// 4.		sum -=  ((v.pathCount - 1)  * v.TimeInS )
	// 5. return sum
	protected override void Subtree(Vertex source) {
		int sum = SPCM(source);

		foreach(Vertex v in graph.Values) 
			WriteLine($"Paths from source to {v.Name} count: {v.PathsFromSourceCount}");

		WriteLine($"SPCM result for source vertex is: {sum}");
	}

	private int SPCM(Vertex source) {
		int sum = DuplicitSumDFS(source);
		// CountPathsFromSource(source);
		foreach (Vertex v in graph.Values)
			sum -= ((v.PathsFromSourceCount - 1) * v.TimeInSeconds);
		return sum;
	}

	int DuplicitSumDFS(Vertex source) {
		source.PathsFromSourceCount++;
		int sum = source.TimeInSeconds;
		foreach(Vertex child in source.Children)
			sum += DuplicitSumDFS(child);
		WriteLine($"Duplicit sum of {source.Name} is {sum}");
		return sum;	
	}
	/*
	void CountPathsFromSource(Vertex source) { 
		source.PathsFromSourceCount++;
		foreach(Vertex child in source.Children)
			CountPathsFromSource(child);
	}*/
}