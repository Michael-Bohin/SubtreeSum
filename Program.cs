using static System.Console;

WriteLine("Hello, Subtree World!");

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

SubtreeCrawler sc = new(graph);
sc.CalculateSubtreeSums();
sc.PrintResult();

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

	public Vertex(string name, List<string> children) {
		Name = name;
		StringChildren = children;
	}
}

class SubtreeCrawler {
	Dictionary<string, Vertex> graph = new();

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

	private void Subtree(Vertex v) {
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
		while(temp!.Color == Color.Black)
			temp = temp.LastBlackParent!;
		return temp;
	}

	public void PrintResult() {
		WriteLine("Subtree sums:");
		foreach(Vertex v in graph.Values)
			WriteLine($"{v.Name}: {v.SubtreeSum}");
	}
}
