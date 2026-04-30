
import Containers from "../../../types/Interfaces.ts";
import { Ref } from "vue";

async function fetchAllContainers(): Promise<Containers[]> {
    try {
        const response = await fetch("http://localhost:5181/api/Container");
        if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

        const data = await response.json();
        if (!data) return [];
        
        const containersData: Containers[] = data.map((item: any) => ({
            containerId: item.containerId || 0,
            containerCode: item.id.name || "",
            containerName: item.containerName.name || "",
            containerVolume: item.containerVolume || 0,
            activate: item.activate || false
        })).sort((a, b) => a.containerId - b.containerId);

        /*
        const containersData: Containers[] = data.$values.map((item: any) => ({
            rawId: item.rawId || 0,
            name: item.name || "",
            info: item.info || ""
        })).sort((a, b) => a.rawId - b.rawId);
*/
        return containersData;
    } catch (err: unknown) {
        console.error("Erro no fetch:", err);
        throw err;
    }
}

async function getByIdContainers(id: number): Promise<Containers> {
    try {
        const response = await fetch(`http://localhost:5181/api/Container/${id}`);
        if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

        const data = await response.json();
        console.log(data);

        if (!data) throw new Error("Contentor não encontrado");

        const iteminContainers = Array.isArray(data.iteminContainers) ? data.iteminContainers : [];
        if (iteminContainers.length === 0) {
            console.log('iteminContainers está vazio ou não existe.');
        }

        const container: Containers = {
            containerId: data.containerId || 0,
            containerCode: data.id?.name || "",
            containerName: data.containerName?.name || "",
            containerVolume: data.containerVolume || 0,
            activate: data.activate || false,
            containerWeight: data.containerWeight || null,
            iteminContainers: iteminContainers.map((item: any) => ({
                itemInContainerId: item.itemInContainerId || 0,
                itemsOfRawMaterial: item.itemsOfRawMaterial ? item.itemsOfRawMaterial.map((rawMaterial: any) => ({
                    itemRawId: rawMaterial.itemRawId || 0,
                    code: rawMaterial.code?.name || "Não especificado",
                    quantity: rawMaterial.quantity || 0,
                    unit: rawMaterial.unit || '',
                    lotOfRawMaterial: {
                        lotId: rawMaterial.lotOfRawMaterial?.lotId || 0,
                        lotNumber: rawMaterial.lotOfRawMaterial?.lotNumber?.name || "Não especificado",
                        lotQuantity: rawMaterial.lotOfRawMaterial?.lotQuantity || 0,
                        lotUnit: rawMaterial.lotOfRawMaterial?.lotUnit || '',
                        rawMaterials: {
                            rawId: rawMaterial.lotOfRawMaterial?.rawMaterials?.rawId || 0,
                            name: rawMaterial.lotOfRawMaterial?.rawMaterials?.name || "Não especificado",
                            info: rawMaterial.lotOfRawMaterial?.rawMaterials?.info || ''
                        },
                        historicalWeights: rawMaterial.lotOfRawMaterial?.historicalWeights || []
                    }
                })) : []
            })),
        };

        return container;
    } catch (err: unknown) {
        console.error("Erro ao buscar contentor por ID:", err);
        throw err;
    }
}

async function handleSubmit(
    containerData: Containers,
    selectedContainer: Ref<Containers | null>,
    showModal: Ref<boolean>
): Promise<Containers[]> {
    try {

        const method = selectedContainer.value ? "PUT" : "POST";
        const { containerId, ...dataToSend } = containerData;

        const url = "http://localhost:5181/api/Container";

        const response = await fetch(url, {
            method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dataToSend)
        });

        if (!response.ok) throw new Error("Falha ao salvar");

        showModal.value = false;
        selectedContainer.value = null;
        return await fetchAllContainers();
    } catch (err: unknown) {
        console.error("Erro ao salvar:", err);
        throw err;
    }
}

async function deleteContainers(
    id: number,
    showDeleteModal: Ref<boolean>
): Promise<Containers[]> {
    try {
        const response = await fetch(`http://localhost:5181/api/Container/${id}`, {
            method: "DELETE"
        });

        if (!response.ok) throw new Error("Falha ao excluir");

        showDeleteModal.value = false;
        return await fetchAllContainers();
    } catch (err: unknown) {
        console.error("Erro ao excluir:", err);
        throw err;
    }
}

function openEdit(Containers: Containers, selectedContainer: Ref<Containers | null>, showModal: Ref<boolean>): void {
    selectedContainer.value = Containers;
    showModal.value = true;
}

function openDelete(id: number, currentContainerId: Ref<number | null>, showDeleteModal: Ref<boolean>): void {
    currentContainerId.value = id;
    showDeleteModal.value = true;
}

export { fetchAllContainers, getByIdContainers,handleSubmit, deleteContainers, openEdit, openDelete };