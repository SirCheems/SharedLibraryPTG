namespace Shared.Http; 
 
using System.Net;

//Tipo de valor devuelto en el caso de éxito.
public class Result<T>
{
    //Bool para indicar si el resultado representa un error. 
    public bool IsError { get; }

    //Exeption asociada al error, en el caso que exista. 
    public Exception? Error { get; }

    //Valor devuelto en el caso de un sucess. 
    public T? Payload { get; }


    //Código del HTTP status que representa el resultado.
    public int StatusCode { get; }

    //Crea un resultado que representa un error. 
    public Result(Exception error, int statusCode =
     (int)HttpStatusCode.InternalServerError)
    {
        IsError = true;
        Error = error;
        Payload = default(T);
        StatusCode = statusCode;
    }

    //Crea un resultado exitoso con un valor.
    public Result(T payload, int statusCode = (int)HttpStatusCode.OK)
    {
        IsError = false;
        Error = null;
        Payload = payload;
        StatusCode = statusCode;
    }
} 

