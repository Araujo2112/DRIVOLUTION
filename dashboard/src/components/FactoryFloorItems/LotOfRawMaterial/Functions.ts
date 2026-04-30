import type {LotRawMaterial} from "../../../types/Interfaces.ts";

async function fetchLots(): Promise<void> {
    try {
        const response = await fetch("http://localhost:5181/api/LotOfRawMaterial");
        if (!response.ok) throw new Error(`Erro HTTP: ${response.status}`);

        const data = await response.json();

        const lotData: LotRawMaterial[] = data.$values.map((item: any) => ({
            lotId: item.lotId,
            id: item.id?.name || "",
            lotNumber: item.lotNumber?.name || "",
            lotQuantity: item.lotQuantity,
            lotUnit: item.lotUnit,
            rawMaterialId: item.rawMaterialId
        }));

        lotData.sort((a, b) => a.lotId - b.lotId);
        lotsRawMaterial.value = lotData;

        console.log(lotData);

    } catch (err) {
        error.value = err instanceof Error ? err.message : "Erro desconhecido";
    }
}

async function handleSubmit(lotData: LotRawMaterial): Promise<void> {
    try {
        const method = selectedLot.value ? "PUT" : "POST";
        const response = await fetch("http://localhost:5181/api/LotOfRawMaterial", {
            method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(lotData)
        });

        if (!response.ok) throw new Error("Falha ao salvar lote");

        await fetchLots();
        showModal.value = false;
        selectedLot.value = null;

    } catch (err) {
        error.value = err instanceof Error ? err.message : "Erro ao salvar";
    }
}


async function deleteLot(): Promise<void> {
    if (!currentLotCode.value) return;

    try {
        const response = await fetch(
            `http://localhost:5181/api/LotOfRawMaterial/${currentLotCode.value}`,
            { method: "DELETE" }
        );

        if (!response.ok) throw new Error("Falha ao excluir lote");

        await fetchLots();
        currentLotCode.value = null;
        showDeleteModal.value = false;

    } catch (err) {
        error.value = err instanceof Error ? err.message : "Erro ao excluir";
    }
}

function openEdit(lot: LotRawMaterial): void {
    selectedLot.value = lot;
    showModal.value = true;
}

function openDelete(lotNumber: string): void {
    currentLotCode.value = lotNumber;
    showDeleteModal.value = true;
}


export { openEdit, openDelete, deleteLot,handleSubmit, fetchLots};