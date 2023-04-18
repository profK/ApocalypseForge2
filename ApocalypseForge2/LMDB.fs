module ApocalypseForge2.LMDB

open FSharp.Json
open LightningDB

type Transaction =
    struct
        val db:LightningDatabase
        val txn:LightningTransaction
        new(db,tx) = { db=db; txn=tx}
    end

let env=new LightningEnvironment("keystore.lmdb");
let OpenTransaction(dbname) =
     let tx =env.BeginTransaction()
     let db = tx.OpenDatabase(configuration=(DatabaseConfiguration(Flags = DatabaseOpenFlags.Create)))
     Transaction(db,tx)
          

let  CommitTransaction(txn:Transaction) =
    txn.txn.Commit()
    
let AbortTransaction(txn:Transaction) =
    txn.txn.Abort()
    ()
    
let PutBytes (key:string, value:byte[]) (tx:Transaction)  =
    tx.txn.Put(tx.db, System.Text.Encoding.ASCII.GetBytes(key) ,value) |> ignore
    tx
     
let PutString (key:string,value:string) (tx:Transaction)  =
    PutBytes (key,System.Text.Encoding.ASCII.GetBytes(value)) |> ignore
    tx
 
let GetBytes (key:string) (tx:Transaction)  =
    tx.txn.Get (tx.db, System.Text.Encoding.ASCII.GetBytes(key))
    |> function
        | (code, k , v ) -> v
        
let GetString (key:string) (tx:Transaction)  =
    tx.txn.Get (tx.db, System.Text.Encoding.ASCII.GetBytes(key))
    |> function
        | (code, k , v ) -> string(v)        

let Put<'T>(key:string, value:'T) (tx:Transaction)=
    let valStr = Json.serialize(value)
    PutString (key, valStr) tx
    
let Get<'T>(key:string) (tx:Transaction) : 'T=
    let string = GetString key tx
    Json.deserialize(string)    
    