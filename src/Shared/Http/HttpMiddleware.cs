namespace Shared.Http;

using System.Collections;
using System.Net;

// Delegate que define la firma de un Middleware HTTP
// Cada middleware recibe:
// - req: el HttpListenerRequest actual
// - res: el HttpListenerResponse para responder
// - props: un Hashtable para almacenar datos de la request a lo largo del pipeline
// - next: una función que llama al siguiente middleware en la cadena
public delegate Task HttpMiddleware(
    HttpListenerRequest req,
    HttpListenerResponse res,
    Hashtable props,
    Func<Task> next
);