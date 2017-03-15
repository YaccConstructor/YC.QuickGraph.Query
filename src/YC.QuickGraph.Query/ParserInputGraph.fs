﻿namespace YC.QuickGraph.Query

open AbstractAnalysis.Common
open QuickGraph
open System.Runtime.CompilerServices

module MyGraph =

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

        member val InitStates = initialVertices 
        member val FinalStates = finalVertices with get, set
        member val ObjToString = match edgeObjToString with
                                 | Some func -> func
                                 | None -> fun value -> value.ToString()

        member inline this.StringToToken str = str |> int

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

    type LinearInput (initialPositions, input:array<int<token>>) =
        interface IParserInput with
            member this.InitialPositions = initialPositions
            [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
            member this.ForAllOutgoingEdges curPosInInput pFun =
                if int curPosInInput < input.Length
                then pFun input.[int curPosInInput] (curPosInInput + 1<positionInInput>)

        member this.Input = input

        new (input:array<int<token>>) = LinearInput ([|0<positionInInput>|], input)

    type SimpleGraphInput<'tagType> (initialPositions, getTokenFromTag:'tagType -> int<token>) =
        inherit AdjacencyGraph<int, TaggedEdge<int, 'tagType>>()
        interface IParserInput with
            member this.InitialPositions = initialPositions
            [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
            member this.ForAllOutgoingEdges curPosInInput pFun =
                let outEdges = int curPosInInput |> this.OutEdges
                outEdges
                |> Seq.iter 
                    (fun e ->
                        pFun (getTokenFromTag e.Tag) (e.Target * 1<positionInInput>)
                    )
