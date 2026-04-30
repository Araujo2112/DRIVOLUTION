using System;
using Python.Runtime;

public class PythonCodeContainer
{
    public string PythonCode { get; private set; }

    public PythonCodeContainer()
    {
        PythonCode = @"
from pydantic import BaseModel, EmailStr, ValidationError

class User(BaseModel):
    id: int
    name: str
    email: EmailStr

def validate_user(data: dict):
    try:
        user = User(**data)
        return {'valid': True, 'data': user.dict()}
    except ValidationError as e:
        return {'valid': False, 'errors': e.errors()}
";
    }

    public void ExecuteValidation(string jsonData)
    {
        try
        {
            PythonEngine.Initialize();

            using (Py.GIL())
            {
                // Executar o código Python
                PythonEngine.Exec(PythonCode);

                // Importar o escopo principal Python
                dynamic pyScope = Py.Import("__main__");

                // Obter a função validate_user
                dynamic validate_user = pyScope.GetAttr("validate_user");

                // Importar o módulo JSON do Python
                dynamic json = Py.Import("json");
                dynamic data = json.loads(jsonData);

                // Validar os dados JSON
                dynamic result = validate_user(data);

                // Processar o resultado
                if (result["valid"].As<bool>())
                {
                    Console.WriteLine("JSON válido!");
                    Console.WriteLine($"Dados: {result["data"]}");
                }
                else
                {
                    Console.WriteLine("JSON inválido!");
                    foreach (var error in result["errors"])
                    {
                        Console.WriteLine(error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao executar o código Python: {ex.Message}");
        }
        finally
        {
            PythonEngine.Shutdown();
        }
    }
}
