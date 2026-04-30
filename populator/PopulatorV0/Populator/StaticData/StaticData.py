import random

dados = {
    # Pessoas
    'nomes': ['Manuel', 'Maria', 'João', 'Ana', 'Carlos', 'Rita', 'Pedro', 'Luísa', 'Miguel', 'Sofia'],
    'apelidos': ['Silva', 'Rodrigues', 'Ferreira', 'Pereira', 'Costa', 'Gomes', 'Martins', 'Santos', 'Almeida',
                 'Oliveira'],

    # Contentores
    'contentores': ['Container_1', 'Container_2', 'Container_3', 'Container_4', 'Container_5'],

    # Secções da fábrica (têxtil)
    'seccoes_fabrica': ['Fiação', 'Tecelagem', 'Tinturaria', 'Acabamento', 'Inspeção de Qualidade'],

    # Produtos
    'produtos': [
        {'nome': 'Polo', 'info': '100% algodão penteado, várias cores, tamanhos S a XXL'},
        {'nome': 'Tecido_Sarja', 'info': '75% algodão / 25% poliéster, 280 g/m²'},
        {'nome': 'Lençol_Casal', 'info': 'Algodão egípcio 300 fios, barra decorativa'},
        {'nome': 'Toalha_Banho', 'info': 'Algodão turco 600 g/m², elevada absorção'},
        {'nome': 'Calcas_Ganga', 'info': 'Denim leve, 2% elastano para conforto'}
    ],

    # Matérias-primas
    'materias_primas': [
        {'nome': 'Algodão Orgânico', 'info': 'Fibra longa, certificação GOTS'},
        {'nome': 'Fio de Poliéster', 'info': 'Ré-Poliéster 50d/72f, alta tenacidade'},
        {'nome': 'Corante Azul Índigo', 'info': 'Reativo para tingimento em padrões xadrez'},
        {'nome': 'Elastano', 'info': 'Fio 40D, alongamento até 500%'},
        {'nome': 'Amido Solúvel', 'info': 'Utilizado no pré-encolhimento da tecelagem'}
    ],

    # Pontos de controlo
    'pontos_controlo': [
        'Inspeção Visual Inicial',
        'Verificação da Tensão do Tear',
        'Teste de Cor antes da Secagem',
        'Controlo de Gramagem',
        'Análise da Resistência à Tração'
    ],

    # Lotes
    'lotes': [
        {'numero_lote': 'LOT20250701-001', 'quantidade': 500, 'unidade': 'kg'},
        {'numero_lote': 'LOT20250701-002', 'quantidade': 1200, 'unidade': 'm'},
        {'numero_lote': 'LOT20250630-003', 'quantidade': 350, 'unidade': 'kg'},
        {'numero_lote': 'LOT20250629-004', 'quantidade': 800, 'unidade': 'm'},
        {'numero_lote': 'LOT20250701-005', 'quantidade': 200, 'unidade': 'kg'}
    ],

    'lotes_produto': [
        {
            "lotNumber": "urn:ngsi-ld:ProductLot:172",
            "lotUnit": "kilo",
            "lotQuantity": 10,
            "ready": True,
            "productLotId": "urn:ngsi-ld:ProductLot:172"
        },
        {
            "lotNumber": "urn:ngsi-ld:ProductLot:173",
            "lotUnit": "meter",
            "lotQuantity": 50,
            "ready": False,
            "productLotId": "urn:ngsi-ld:ProductLot:173"
        },
        {
            "lotNumber": "urn:ngsi-ld:ProductLot:174",
            "lotUnit": "kilo",
            "lotQuantity": 200,
            "ready": True,
            "productLotId": "urn:ngsi-ld:ProductLot:174"
        },
        {
            "lotNumber": "urn:ngsi-ld:ProductLot:175",
            "lotUnit": "meter",
            "lotQuantity": 150,
            "ready": True,
            "productLotId": "urn:ngsi-ld:ProductLot:175"
        },
        {
            "lotNumber": "urn:ngsi-ld:ProductLot:176",
            "lotUnit": "kilo",
            "lotQuantity": 75,
            "ready": False,
            "productLotId": "urn:ngsi-ld:ProductLot:176"
        }
    ],

    # Fases de produção
    'fases_producao': [
        {'duracao_horas': 2.5, 'info': 'Preparação do tear e colocação do urdume'},
        {'duracao_horas': 5.0, 'info': 'Tecelagem do tecido base'},
        {'duracao_horas': 1.5, 'info': 'Remoção e inspeção do tecido cru'},
        {'duracao_horas': 3.0, 'info': 'Tingimento e lavagem'},
        {'duracao_horas': 4.0, 'info': 'Secagem e acabamento final'}
    ],

    # Processos de fabrico
    'processos_fabricacao': [
        {
            'nome': 'Fabrico de Polo',
            'info': 'Processo completo de fabricação de Polo, incluindo fiação, tecelagem, tinturaria, acabamento e inspeção'
        },
        {
            'nome': 'Fabrico de Tecido Sarja',
            'info': 'Processo completo de fabricação de tecido sarja, incluindo fiação, tecelagem, tinturaria, acabamento e inspeção'
        },
        {
            'nome': 'Fabrico de Lençol de Casal',
            'info': 'Processo completo de fabricação de lençol de casal, incluindo fiação, tecelagem, tinturaria, acabamento e inspeção'
        },
        {
            'nome': 'Fabrico de Toalha de Banho',
            'info': 'Processo completo de fabricação de toalha de banho, incluindo fiação, tecelagem, tinturaria, acabamento e inspeção'
        },
        {
            'nome': 'Fabrico de Calças de Ganga',
            'info': 'Processo completo de fabricação de calças de ganga, incluindo fiação, tecelagem, tinturaria, acabamento e inspeção'
        }
    ]
}


def random_index(nome_conjunto: str) -> int:
    """Retorna um índice aleatório válido para a lista correspondente em 'dados'.
    Args:
        nome_conjunto: Chave do dicionário 'dados'.
    Returns:
        Índice aleatório.
    Raises:
        KeyError: Se o conjunto não existir.
        ValueError: Se o conjunto estiver vazio.
    """
    try:
        lista = dados[nome_conjunto]
    except KeyError:
        raise KeyError(f'Conjunto "{nome_conjunto}" não encontrado em dados.')
    if not lista:
        raise ValueError(f'Conjunto "{nome_conjunto}" está vazio.')
    return random.randrange(len(lista))
