/*
Define conversores personalizados para controlar o formato das datas e adaptar propriedades JSON de diferentes formatos, 
facilitando a comunicação entre a aplicação e os dados recebidos através do FIWARE.
*/
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Drivolution.Converters
{
    // Conversor personalizado para valores DateTime
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        // Formato obrigatório das datas no JSON
        private readonly string _dateFormat = "yyyy-MM-ddTHH:mm:ss";

        // Converte uma data recebida em JSON para DateTime
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // Confirma que o valor recebido é uma string
            if (reader.TokenType == JsonTokenType.String)
            {
                // Obtém o texto da data
                string? dateString = reader.GetString();

                // Tenta converter o texto exatamente no formato definido
                if (
                    dateString != null &&
                    DateTime.TryParseExact(
                        dateString,
                        _dateFormat,
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime date
                    )
                )
                {
                    // Se o formato estiver correto, devolve a data convertida
                    return date;
                }
                else
                {
                    // Se a data tiver outro formato, lança um erro
                    throw new JsonException(
                        $"Invalid date format. Expected format is {_dateFormat}."
                    );
                }
            }

            // Se o valor recebido não for uma string, lança um erro
            throw new JsonException(
                "Invalid JSON format for DateTime."
            );
        }

        // Converte um DateTime para texto JSON
        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            // Escreve a data no formato definido
            writer.WriteStringValue(
                value.ToString(_dateFormat)
            );
        }
    }


    // Conversor personalizado para propriedades de texto
    public class PropertyConverter : JsonConverter<string>
    {
        // Lê uma propriedade recebida em JSON
        public override string Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            // Se o JSON recebido for um objeto
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Lê o objeto JSON completo
                using (
                    JsonDocument doc =
                        JsonDocument.ParseValue(ref reader)
                )
                {
                    var root = doc.RootElement;

                    // Tenta obter o valor da propriedade "value"
                    if (
                        root.TryGetProperty(
                            "value",
                            out JsonElement valueProperty
                        )
                    )
                    {
                        return valueProperty.GetString() ?? "";
                    }

                    // Se não existir "value", tenta "object"
                    if (
                        root.TryGetProperty(
                            "object",
                            out JsonElement objectProperty
                        )
                    )
                    {
                        return objectProperty.GetString() ?? "";
                    }

                    // Se não existir "object", tenta "SectorId"
                    if (
                        root.TryGetProperty(
                            "SectorId",
                            out JsonElement sectoridProperty
                        )
                    )
                    {
                        return sectoridProperty.GetString() ?? "";
                    }
                }
            }

            // Se o valor já for uma string simples,
            // devolve-a diretamente
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? "";
            }

            // Se não tiver nenhum dos formatos esperados,
            // lança um erro
            throw new JsonException(
                "Invalid JSON format for the property."
            );
        }

        // Converte uma string C# para um objeto JSON
        public override void Write(
            Utf8JsonWriter writer,
            string value,
            JsonSerializerOptions options)
        {
            // Começa a escrever um objeto JSON
            writer.WriteStartObject();

            // Verifica uma condição específica para decidir
            // que estrutura JSON será criada
            if (
                value.Contains(
                    "some_specific_type_condition"
                )
            )
            {
                // Escreve no formato de uma Property
                writer.WriteString(
                    "type",
                    "Property"
                );

                writer.WriteString(
                    "value",
                    value
                );
            }
            else
            {
                // Caso contrário, escreve apenas o nome
                writer.WriteString(
                    "name",
                    value
                );
            }

            // Termina o objeto JSON
            writer.WriteEndObject();
        }
    }
}