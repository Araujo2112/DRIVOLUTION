import { type Ref } from 'vue';
import {ItemOfRawMaterial, LotRawMaterial} from "../../../types/Interfaces.ts";

const handleApiResponse = async (response: Response) => {
    console.log('[API] Response:', response.status);
    if (!response.ok) {
        const error = await response.json();
        console.error('[API] Error:', error);
        throw new Error(error.title || 'Erro na operação');
    }
    return response.json();
};

export const fetchAllItems = async (): Promise<ItemOfRawMaterial[]> => {
    console.log('[API] Buscando itens...');
    try {
        const response = await fetch('http://localhost:5181/api/ItemOfRawMaterial');
        const data = await handleApiResponse(response);

        console.log('[API] Dados brutos:', data);

        const finaldata = data.$values
            .filter(item => {
                const isValid = !!item?.code?.name;
                if (!isValid) {
                    console.warn('[API] Item ignorado - estrutura inválida:', item);
                }
                return isValid;
            })
            .map(item => {
                const { code, ...rest } = item;
                return {
                    ...rest,
                    itemCode: code.name
                } as ItemOfRawMaterial;
            });

        console.log('[API] Dados processados:', finaldata);
        return finaldata;

    } catch (error) {
        console.error('[API] Erro crítico:', error);
        throw new Error('Falha na comunicação com o servidor');
    }
};

export const fetchLots = async (): Promise<LotRawMaterial[]> => {
    console.log('[API] Fetching lots...');
    const response = await fetch('http://localhost:5181/api/LotOfRawMaterial');
    const data = await handleApiResponse(response);
    return data.$values.map(lot => ({
        ...lot,
        lotNumber: lot.lotNumber
    }));
};

export const handleSubmit = async (
    data: ItemOfRawMaterial,
    selectedItem: Ref<ItemOfRawMaterial | null>
): Promise<ItemOfRawMaterial[]> => {
    console.log('[API] Submitting:', data);
    const isEditing = !!selectedItem.value;
    const url = isEditing
        ? `http://localhost:5181/api/ItemOfRawMaterial`
        : 'http://localhost:5181/api/ItemOfRawMaterial';

    const body = {
        itemCode: data.itemCode,
        quantity: data.quantity,
        unit: data.unit,
        lotOfRawMaterialId: data.lotOfRawMaterialId,
        itemInContainerId: data.itemInContainerId
    };

    const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    await handleApiResponse(response);
    return fetchAllItems();
};


export const deleteItemOfRawMaterial = async (itemCode: string): Promise<ItemOfRawMaterial[]> => {
    console.log('[API] Deletando item:', itemCode);
    try {
        const response = await fetch(`http://localhost:5181/api/ItemOfRawMaterial/${itemCode}`, {
            method: 'DELETE'
        });

        const text = await response.text();
        const data = text ? JSON.parse(text) : {};

        if (!response.ok) {
            console.error('[API] Erro detalhado:', data);
            throw new Error(data.title || 'Erro ao excluir item');
        }

        console.log('[API] Item deletado com sucesso');
        return await fetchAllItems(); 

    } catch (error) {
        console.error('[API] Erro completo na exclusão:', error);
        throw new Error('Não foi possível completar a exclusão');
    }
};
