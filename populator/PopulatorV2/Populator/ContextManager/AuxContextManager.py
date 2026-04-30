import os


def pkl_exists() -> bool:
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    pkl_path = os.path.join(base_dir, 'Main', 'populator_context.pkl')
    return os.path.isfile(pkl_path)


def clear_entity(context_instance, entity_type: str) -> None:
    if entity_type in context_instance.storage:
        del context_instance.storage[entity_type]
        print(f"Dados da entidade '{entity_type}' removidos do contexto.")
    else:
        print(f"Nenhum dado encontrado para a entidade '{entity_type}'.")
