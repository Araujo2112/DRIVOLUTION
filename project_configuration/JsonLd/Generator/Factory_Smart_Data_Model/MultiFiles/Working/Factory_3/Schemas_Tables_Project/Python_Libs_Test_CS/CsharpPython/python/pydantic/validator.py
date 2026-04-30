import sys
import json
from pydantic import BaseModel, Field, conint, ValidationError
from typing import List, Optional


class Pet(BaseModel):
    name: Optional[str] = None
    age: Optional[int] = None

class Person(BaseModel):
    first_name: str = Field(..., description="The person's first name.")
    last_name: str = Field(..., description="The person's last name.")
    age: Optional[conint(ge=0)] = Field(None, description='Age in years.')
    pets: Optional[List[Pet]] = None
    comment: Optional[str] = None

def validate_json(data: str):
    try:
       
        json_data = json.loads(data)

        person = Person(**json_data)

        print("JSON válido!")
        print(person)

    except ValidationError as e:
        print("Erro de validação:")
        print(e.json())

    except json.JSONDecodeError:
        print("Erro ao descodificar JSON!")

    except Exception as e:
        print(f"Erro: {str(e)}")

if __name__ == "__main__":
    json_data = sys.stdin.read()
    validate_json(json_data)
