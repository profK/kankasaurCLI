module JournalToLoader.JsonFormatting
open System.Text.Json
open System.Text.Json.Serialization

type MarkerRec = {
    name : string
    id : string
    pageId : string
    iconPath : string
    x : float
    y : float    
}

type MarkerGroupRec = {
    name : string
    pages : MarkerRec seq 
}
type OffsetRec = {
    x : float
    y : float
}
type MapRec = {
    name:string
    groups: MarkerGroupRec seq
 }