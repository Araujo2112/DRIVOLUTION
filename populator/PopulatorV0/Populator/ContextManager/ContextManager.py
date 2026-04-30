# ContextManager.py
import pickle
import random
import re
from typing import Dict, List, Any, Optional


class ContextManager:
    def __init__(self):
        self.storage: Dict[str, List[Dict]] = {}
        self.entity_relations: Dict[str, List[str]] = {}

    def add_entity(self, entity_type: str, entity_id: Any, entity_data: dict, db_id: int):
        """
        Armazena uma entidade criada no contexto.
        - entity_id: identificador externo (URN, string, etc)
        - db_id: id da base de dados (int), se não for passado, tenta extrair de entity_data
        """

        print("Databse id is", db_id)

        if entity_type not in self.storage:
            self.storage[entity_type] = []
        self.storage[entity_type].append({
            'id': entity_id,  # URN, string, etc
            'db_id': db_id,  # ID da base de dados (int)
            'data': entity_data  # Resposta completa da API
        })

    def get_random_db_id(self, entity_type: str) -> int:
        """Retorna um db_id aleatório de uma entidade existente"""
        entities = self.storage.get(entity_type, [])
        valid = [e['db_id'] for e in entities if e.get('db_id') is not None]
        if not valid:
            raise ValueError(f"Nenhum(a) {entity_type} com db_id disponível no contexto")
        return random.choice(valid)

    def get_entity_data_by_db_id(self, entity_type: str, db_id: int) -> Optional[dict]:
        """Recupera dados completos de uma entidade pelo db_id"""
        for entity in self.storage.get(entity_type, []):
            if entity.get('db_id') == db_id:
                return entity['data']
        return None

    def get_sorted_db_ids_by_field(self, entity_type: str, field: str, numeric_urn: bool = False) -> list:
        """
        Retorna uma lista de db_ids ordenados pelo campo especificado.
        """
        sorted_entities = self.get_sorted_entities_by_field(entity_type, field, numeric_urn)
        return [e['db_id'] for e in sorted_entities if e.get('db_id') is not None]

    def get_sorted_db_ids(self, entity_type: str) -> list:
        """
        Retorna uma lista de db_ids ordenados crescentemente.
        """
        entities = self.storage.get(entity_type, [])
        print("Entities", entities)
        db_ids = [e['db_id'] for e in entities if e.get('db_id') is not None]
        print("db_ids", db_ids)
        return sorted(db_ids)

    def get_random_entity_id(self, entity_type: str) -> int:
        """Retorna um ID aleatório de uma entidade existente"""
        entities = self.storage.get(entity_type, [])
        if not entities:
            raise ValueError(f"Nenhum(a) {entity_type} disponível no contexto")
        return random.choice(entities)['id']

    def get_entity_data(self, entity_type: str, entity_id: int) -> Optional[dict]:
        """Recupera dados completos de uma entidade pelo ID"""
        for entity in self.storage.get(entity_type, []):
            if entity['id'] == entity_id:
                return entity['data']
        return None

    def get_entity_by_field(
            self,
            entity_type: str,
            field_name: str,
            field_value: Any
    ) -> Optional[dict]:
        """Busca entidade por um campo específico (ex: containerName)"""
        for entity in self.storage.get(entity_type, []):
            if entity['data'].get(field_name) == field_value:
                return entity
        return None

    def save_context(self, filename: str = 'populator_context.pkl'):
        """Salva o contexto em disco para uso futuro"""
        with open(filename, 'wb') as f:
            pickle.dump({
                'storage': self.storage,
                'entity_relations': self.entity_relations
            }, f)

    def load_context(self, filename: str = 'populator_context.pkl'):
        """Carrega o contexto do disco"""
        try:
            with open(filename, 'rb') as f:
                data = pickle.load(f)
                self.storage = data['storage']
                self.entity_relations = data.get('entity_relations', {})
        except FileNotFoundError:
            print("[ContextManager] Contexto anterior não encontrado. Iniciando novo contexto.")
        except Exception as e:
            print(f"[ContextManager] Erro ao carregar contexto: {str(e)}")

    def define_relationship(self, parent: str, child: str):
        """Define um relacionamento entre entidades"""
        if parent not in self.entity_relations:
            self.entity_relations[parent] = []
        if child not in self.entity_relations[parent]:
            self.entity_relations[parent].append(child)

    # ContextManager.py
    def get_sorted_entity_ids(self, entity_type: str) -> list:
        """Retorna IDs de uma entidade ordenados crescentemente"""
        entities = self.storage.get(entity_type, [])
        ids = [e['id'] for e in entities]
        print(ids)
        return sorted(ids)  # Ordem crescente

    @staticmethod
    def extract_number_from_urn(urn: str) -> int:
        """Extrai número de uma URN no formato 'urn:ngsi-ld:...:123'"""
        match = re.search(r':(\d+)$', urn)
        return int(match.group(1)) if match else -1

    def get_sorted_entities_by_field(self, entity_type: str, field: str, numeric_urn: bool = False) -> list:
        """
        Retorna uma lista de entidades ordenadas pelo campo especificado.
        Se numeric_urn=True, extrai o número do campo (assumindo formato URN).
        """
        entities = self.storage.get(entity_type, [])
        if numeric_urn:
            # Ordena extraindo número do campo (ex: 'name' com URN)
            sorted_entities = sorted(
                entities,
                key=lambda e: self.extract_number_from_urn(e['data'].get(field, ''))
            )
        else:
            # Ordena pelo valor do campo diretamente
            sorted_entities = sorted(
                entities,
                key=lambda e: e['data'].get(field, '')
            )
        return sorted_entities

    def get_sorted_entity_ids_by_field(self, entity_type: str, field: str, numeric_urn: bool = False) -> list:
        """
        Retorna uma lista de IDs ordenados pelo campo especificado.
        Se numeric_urn=True, extrai o número do campo (assumindo formato URN).
        """
        sorted_entities = self.get_sorted_entities_by_field(entity_type, field, numeric_urn)
        return [e['id'] for e in sorted_entities]


# Instância global para ser usada em todo o projeto
context = ContextManager()
