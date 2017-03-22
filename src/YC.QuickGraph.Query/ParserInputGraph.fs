namespace YC.QuickGraph.Query

open AbstractAnalysis.Common
open QuickGraph
open System.Runtime.CompilerServices

module GraphsImpl =

    type LexerEdge<'l ,'br  when 'l: equality> (s,e,t) =
        inherit TaggedEdge<int,Option<'l*'br>>(s,e,t)
        let l,br =
            match t with
            | Some (l,br) -> Some l, Some br
            | None -> None, None

        member this.BackRef = br
        member this.Label = l

    type ParserEdge<'EdgeObject>(s, e, t)=
        inherit TaggedEdge<int, 'EdgeObject>(s, e, t)

    type ParserInputGraph<'EdgeObject>(initialVertices : int[], finalVertices : int[], ?edgeObjToString: ('EdgeObject -> string)) = 
        inherit AdjacencyGraph<int, ParserEdge<'EdgeObject>>()

        let StringToToken str = str |> int

        member val InitStates = initialVertices 
        member val FinalStates = finalVertices with get, set
        member val ObjToString = match edgeObjToString with
                                 | Some func -> func
                                 | None -> fun value -> value.ToString()

        member this.PrintToDot name (*(tokenToString : 'token -> string) (numToToken : int -> 'token)*) = 
            use out = new System.IO.StreamWriter (name : string)
            out.WriteLine("digraph AST {")
            out.WriteLine "rankdir=LR"
            for i = 0 to this.VertexCount - 1 do
                out.Write (i.ToString() + "; ")
            out.WriteLine()
            for i in this.Vertices do
                let edges = this.OutEdges i
                for e in edges do
                    let tokenName = e.Tag |> this.ObjToString(*numToToken |> tokenToString*)
                    out.WriteLine (e.Source.ToString() + " -> " + e.Target.ToString() + "[label=\"" + tokenName + "\"]")
            out.WriteLine("}")
            out.Close()

        new (initial : int, final : int, ?edgeObjToString: ('EdgeObject -> string)) = 
            let objToStr = match edgeObjToString with
                           | Some func -> func
                           | None -> fun value -> value.ToString()
            ParserInputGraph<_>([|initial|], [|final|], objToStr)

        new (n : int, ?edgeObjToString: ('EdgeObject -> string)) =
            let allVerticles = [|for i in 0 .. n - 1 -> i|]
            let objToStr = match edgeObjToString with
                           | Some func -> func
                           | None -> fun value -> value.ToString()
            ParserInputGraph<_>(allVerticles, allVerticles, objToStr)

        interface IParserInput with
            member this.InitialPositions = 
                Array.map (fun x -> x * 1<positionInInput>) this.InitStates
            member this.ForAllOutgoingEdges curPosInInput pFun =
                let outEdges = int curPosInInput |> this.OutEdges
                outEdges |> Seq.iter 
                    (fun e ->
                        pFun ((e.Tag |> this.ObjToString |> StringToToken) * 1<token>) (e.Target * 1<positionInInput>)
                    )
