import {RawMaterial} from "@/types/Interfaces";
import {Ref} from "vue";

async function fetchAllRawMaterials(): Promise<RawMaterial[]> {
    try {
        const response = await fetch("http://localhost:5181/api/RawMaterial");
        if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

        const data = await response.json() ?? []

        return data.map((item: any) => ({
            rawId: item.rawId || 0,
            name: item.name || "",
            info: item.info || ""
        })).sort((a, b) => a.rawId - b.rawId)
    } catch (err: unknown) {
        console.error("Erro no fetch:", err);
        throw err;
    }
}

async function handleSubmit(
    rawMaterialData: RawMaterial,
    selectedRawMaterial: Ref<RawMaterial | null>,
    showModal: Ref<boolean>
): Promise<RawMaterial[]> {
    try {
        const method = selectedRawMaterial.value ? "PUT" : "POST";
        const url = selectedRawMaterial.value
            ? `http://localhost:5181/api/RawMaterial`
            : "http://localhost:5181/api/RawMaterial";

        const response = await fetch(url, {
            method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(rawMaterialData)
        });

        if (!response.ok) throw new Error("Falha ao salvar");

        showModal.value = false;
        selectedRawMaterial.value = null;
        return await fetchAllRawMaterials();
    } catch (err: unknown) {
        console.error("Erro ao salvar:", err);
        throw err;
    }
}

async function deleteRawMaterial(
    id: number,
    showDeleteModal: Ref<boolean>
): Promise<RawMaterial[]> {
    try {
        const response = await fetch(`http://localhost:5181/api/RawMaterial/${id}`, {
            method: "DELETE"
        });

        if (!response.ok) throw new Error("Falha ao excluir");

        showDeleteModal.value = false;
        return await fetchAllRawMaterials();
    } catch (err: unknown) {
        console.error("Erro ao excluir:", err);
        throw err;
    }
}

function openEdit(rawMaterial: RawMaterial, selectedRawMaterial: Ref<RawMaterial | null>, showModal: Ref<boolean>): void {
    selectedRawMaterial.value = rawMaterial;
    showModal.value = true;
}
function openDelete(id: number, currentRawMaterialId: Ref<number | null>, showDeleteModal: Ref<boolean>): void {
    currentRawMaterialId.value = id;
    showDeleteModal.value = true;
}

export { fetchAllRawMaterials, handleSubmit, deleteRawMaterial, openEdit, openDelete };