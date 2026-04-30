import pg8000
from datetime import datetime, timedelta
from faker import Faker
from random import randint, choice

fake = Faker("pt_PT")
NOW = datetime.utcnow()


conn = pg8000.connect(
    host="localhost",
    port=5432,
    database="texpact",
    user="texpact",
    password="texpact"
)
cur = conn.cursor()


products = [
    ("Tecido Ganga", "Para calças jeans", "PROD001"),
    ("Tecido Stretch", "Para roupa desportiva", "PROD002"),
    ("Tecido Algodão", "Para camisas", "PROD003"),
    ("Forro", "Para jaquetas", "PROD004"),
    ("Tecido Técnico", "Para EPI", "PROD005"),
]

raws = [
    ("Algodão Orgânico", "Fibra longa, GOTS"),
    ("Fio de Poliéster", "50d/72f, alta tenacidade"),
    ("Corante Azul Índigo", "Reativo"),
    ("Elastano", "Fio 40D, 500 % alongamento"),
    ("Amido Solúvel", "Pré-encolhimento"),
]

sections = ["Corte", "Costura", "Lavagem", "Secagem", "Acabamento"]
clients = [("Confecções Silva", "123456789"),
           ("Textilar Lda", "987654321"),
           ("Tecidos Premium", "456789123")]


product_section_materials = {
    0: {
        0: [0],
        1: [1],
        2: [2],
        3: [4],
        4: [0, 2],
    },
    1: {
        0: [0],
        1: [1, 3],
        2: [4],
        3: [3],
        4: [1, 3],
    }
}


def execute(sql, params=()):
    cur.execute(sql, params)
    return cur.fetchone()[0] if "RETURNING" in sql else None

def many(sql, seq):
    cur.executemany(sql, seq)


section_ids = []
for i, name in enumerate(sections, 1):
    section_ids.append(
        execute('INSERT INTO "PlantFloorSection" ("SectionCode", name) VALUES (%s,%s) RETURNING "SectionId"',
                (f"SEC{i:02d}", f"Setor {name}"))
    )


phase_ids = []
for i, sid in enumerate(section_ids, 1):
    phase_ids.append(
        execute(
            'INSERT INTO "ManufacturingPhases" ("PhaseInfo","PhaseDuration","PlantFloorSectionId","ManufacturingPhaseId")'
            ' VALUES (%s,%s,%s,%s) RETURNING "Id"',
            (sections[i - 1], randint(10, 60), sid, f"PHASE-{i:02d}"))
    )

container_ids = []
for i in range(1, 3):
    container_ids.append(
        execute('INSERT INTO "Containers" ("ContainerCode","ContainerName","ContainerVolume","Activate")'
                ' VALUES (%s,%s,%s,%s) RETURNING "ContainerId"',
                (f"CONT-{i:03d}", f"Contentor {i}", randint(100, 400), True))
    )

container_localizations = {}
for container_idx, cid in enumerate(container_ids):
    container_localizations[container_idx] = []
    for section_idx, sid in enumerate(section_ids):
        loc_id = execute(
            'INSERT INTO "ContainerLocalization" ("ContainerId","SectionId","Datetime")'
            ' VALUES (%s,%s,%s) RETURNING "Id"',
            (cid, sid, NOW + timedelta(hours=section_idx))
        )
        container_localizations[container_idx].append(loc_id)


for i, sid in enumerate(section_ids, 1):
    execute('INSERT INTO "Checkpoints" ("CheckpointCode","Name","Status","SectionId") VALUES (%s,%s,%s,%s)',
            (f"CKP-{i:02d}", f"Checkpoint {i}", True, sid))

raw_ids = []
for name, info in raws:
    raw_ids.append(
        execute('INSERT INTO "RawMaterial" ("Name","Info") VALUES (%s,%s) RETURNING "RawId"', (name, info))
    )


lot_ids = []
for i, rid in enumerate(raw_ids, 1):
    lot_ids.append(
        execute(
            'INSERT INTO "LotOfRawMaterial" ("LotCode","LotNumber","LotQuantity","LotUnit","RawMaterialId","HistoricalWeights")'
            ' VALUES (%s,%s,%s,%s,%s,%s) RETURNING "LotId"',
            (f"LRM-{i:03d}", f"N{i:04d}", randint(200, 800), "kg", rid, [randint(50, 100), randint(50, 100)]))
    )


product_db_ids = []
for name, info, pid in products:
    product_db_ids.append(
        execute('INSERT INTO "Products" ("Name","Info","ProductId") VALUES (%s,%s,%s) RETURNING "Id"',
                (name, info, pid))
    )


process_ids = []
for i, prod_id in enumerate(product_db_ids, 1):
    pr_id = execute(
        'INSERT INTO "ManufacturingProcesses" ("ProcessName","Info","ProductId")'
        ' VALUES (%s,%s,%s) RETURNING "Id"',
        (f"Processo {i}", "Automático", prod_id)
    )
    process_ids.append(pr_id)
    many('INSERT INTO "ManufacturingProcessPhases" ("ManufacturingPhaseId","ManufacturingProcessId","NumberStepOrder")'
         ' VALUES (%s,%s,%s)',
         [(pid, pr_id, n + 1) for n, pid in enumerate(phase_ids)])


prodlot_ids = []
for i, prod_id in enumerate(product_db_ids, 1):
    prodlot_ids.append(
        execute('INSERT INTO "ProductLots" ("LotNumber","LotUnit","LotQuantity","Ready","ProductLotId","ProductId")'
                ' VALUES (%s,%s,%s,%s,%s,%s) RETURNING "Id"',
                (f"PL-{i:04d}", "m", randint(500, 2000), False, f"PLID-{i:03d}", prod_id))
    )


client_ids = []
for name, nif in clients:
    client_ids.append(
        execute('INSERT INTO "Clients" ("Name","FiscalNumber") VALUES (%s,%s) RETURNING "Id"', (name, nif))
    )


order_products = [0, 1]
order_ids = []

for i in range(2):
    product_idx = order_products[i]

    oid = execute(
        'INSERT INTO "ManufacturingOrders" ("Observations","OrderNumber","ManufacturingOrderId","ClientId",'
        '"ManufacturingProcessId","ProductLotId","SheduleInit")'
        ' VALUES (%s,%s,%s,%s,%s,%s,%s) RETURNING "Id"',
        (fake.sentence(), 1000 + i, f"ORD-{i + 1:03d}", client_ids[i],
         process_ids[product_idx], prodlot_ids[product_idx], NOW)
    )
    order_ids.append(oid)

    execute(
        'INSERT INTO "ManufacturingOrderHistories" ("ManufacturingOrderId","PlantFloorSectionId","DateTime","StatusName")'
        ' VALUES (%s,%s,%s,%s)', (oid, section_ids[0], NOW, "Criada"))

    order_container = container_ids[i]

    product_materials = product_section_materials.get(product_idx, {})

    def gerar_customization_params(product_idx, section_idx, product_name, materials_for_section, raws):
        descricoes_por_produto = {
            0: [
                "Corte do tecido de ganga em moldes padrão",
                "Costura com fio de poliéster resistente",
                "Tingimento com corante azul índigo",
                "Secagem com amido para pré-encolhimento",
                "Finalização e dobra do tecido de ganga"
            ],
            1: [
                "Corte do tecido stretch com precisão",
                "Costura com fio elástico para flexibilidade",
                "Tratamento com amido para preparação do tecido",
                "Secagem com estabilização do elastano",
                "Toque final com reforço elástico"
            ],
            3: [
                "Corte do forro em formato de casaco",
                "Costura com reforço de algodão e poliéster",
                "Tratamento com amido para estabilização",
                "Secagem técnica com controlo térmico",
                "Montagem e inspeção final do casaco"
            ]
        }

        material_nomes = [raws[m][0] for m in materials_for_section]
        fases_do_produto = descricoes_por_produto.get(product_idx, [])
        descricao_fase = fases_do_produto[section_idx] if section_idx < len(fases_do_produto) else "Fase de produção"

        if section_idx == 4:
            return f"Produto final: {product_name} - {descricao_fase}"
        else:
            return f"{descricao_fase}. Matérias-primas usadas: {', '.join(material_nomes)}"


    order_phase_ids = []
    for section_idx, pid in enumerate(phase_ids):
        materials_for_section = product_materials.get(section_idx, [0])
        customization_text = gerar_customization_params(
            product_idx, section_idx, products[product_idx][0], materials_for_section, raws
        )

        op_id = execute(
            'INSERT INTO "ManufacturingOrderPhases" ("CustomizationParams","Quantity","SheduleInit","DateTimeInit","DateTimeEnd",'
            '"ManufacturingOrderId","ManufacturingPhaseId")'
            ' VALUES (%s,%s,%s,%s,%s,%s,%s) RETURNING "Id"',
            (customization_text, randint(50, 100), NOW,
             NOW + timedelta(hours=section_idx),
             NOW + timedelta(hours=section_idx + 1), oid, pid)
        )
        order_phase_ids.append(op_id)


    for section_idx, op_id in enumerate(order_phase_ids):

        materials_for_section = product_materials.get(section_idx, [0])



        for material_idx in materials_for_section:
            item_code = f"IT-{oid}-{section_idx + 1:02d}-{material_idx + 1:02d}"

            item_in_container_id = execute(
                'INSERT INTO "ItemInContainer" ("ItemCode","ContainerId","DateTimeIn","DateTimeOut")'
                ' VALUES (%s,%s,%s,%s) RETURNING "ItemInContainerId"',
                (item_code, order_container,
                 NOW + timedelta(hours=section_idx),
                 NOW + timedelta(hours=section_idx + 1))
            )

            itm_id = execute(
                'INSERT INTO "ItemOfRawMaterial" ("ItemCode","Quantity","Unit","LotOfRawMaterialId","ItemInContainerId",'
                '"ManufacturingOrderPhaseId","ManufacturingOrderId")'
                ' VALUES (%s,%s,%s,%s,%s,%s,%s) RETURNING "ItemRawId"',
                (item_code, randint(10, 30), "kg", lot_ids[material_idx],
                 item_in_container_id, op_id, oid)
            )

            execute(
                'INSERT INTO "ItemLocalization" ("ItemRawId","ContainerLocalizationId","DateTime") VALUES (%s,%s,%s)',
                (itm_id, container_localizations[i][section_idx], NOW + timedelta(hours=section_idx))
            )

for i in range(5):
    emp_id = execute(
        'INSERT INTO "Employees" ("FirstName","LastName","Username","Password","WatchId","ManufacturingPhaseId")'
        ' VALUES (%s,%s,%s,%s,%s,%s) RETURNING "Id"',
        (fake.first_name(), fake.last_name(), f"user{i}", "123", None, phase_ids[i])
    )
    execute('INSERT INTO "SectionAdminModel" ("EmployeeId","PlantFloorSectionId","AssignedDate") VALUES (%s,%s,%s)',
            (emp_id, section_ids[i], NOW))

conn.commit()
cur.close()
conn.close()
print("Populate!")
