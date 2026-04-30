using System;
using System.Diagnostics;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        
        string jsonInput = @"{
            ""first_name"": ""John"",
            ""last_name"": ""Doe"",
            ""age"":""30"",
            ""pets"": [
                { ""name"": ""Rex"", ""age"": 5 },
                { ""name"": ""Fluffy"", ""age"": 2 }
            ],
            ""comment"": ""A wonderful person with great pets!""
        }";
        
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        
        string relativePath = Path.Combine(basePath, "../../../python/pydantic/validator.py");
        
        string absolutePath = Path.GetFullPath(relativePath);

        if (!File.Exists(absolutePath))
        {
            Console.WriteLine("O script Python não foi encontrado no caminho: " + absolutePath);
            return;
        }

        Console.WriteLine("A executar script Python...");

        try
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonInput);
            MemoryStream memoryStream = new MemoryStream(jsonBytes);
            
            string pythonCommand = $"python3 {absolutePath}";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{pythonCommand}\"",
                    RedirectStandardInput = true, 
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            
            Console.WriteLine("A enviar JSON para o Python...");
            memoryStream.CopyTo(process.StandardInput.BaseStream);
            process.StandardInput.Close();
            
            memoryStream.Close();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                Console.WriteLine("JSON válido!");
            }
            else
            {
                Console.WriteLine("JSON inválido!");
                Console.WriteLine(error);
            }

            
            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine(output);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao executar o processo Python: " + ex.Message);
        }
    }
}
