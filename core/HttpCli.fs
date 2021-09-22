module internal Suave.HttpCli

open System.IO
open System.Net

type DefineRequest = HttpWebRequest -> unit

let send (url: string) (authToken: string) meth (define : DefineRequest) = 
    async { 
        let request = WebRequest.Create(url) :?> HttpWebRequest
        request.Method <- meth
        request.UserAgent <- "suave app"    // this line is required for github auth

        if not (String.isEmpty(authToken)) then
          request.Headers.Add("Authorization", sprintf "token %s" authToken)

        do define request

        let! (r:WebResponse) = request.GetResponseAsync()
        use response = r
        let stream = response.GetResponseStream()
        use reader = new StreamReader(stream)
        return reader.ReadToEnd()
    }

let post (url: string) (define : DefineRequest) (data : byte []) = 
    send url "" "POST"
        (fun request ->
        request.ContentType <- "application/x-www-form-urlencoded"
        request.ContentLength <- int64 data.Length

        do define request
        
        use stream = request.GetRequestStream()
        stream.Write(data, 0, data.Length)
        stream.Close()

)

let get (url : string) (authToken: string) = send url authToken "GET"
