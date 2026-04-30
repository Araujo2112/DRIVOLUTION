// Interfaces.ts
export interface RawMaterial {
    rawId: number;
    name: string;
    info: string;
}

export interface LotRawMaterial {
    lotId: number;
    id: string;
    lotNumber: { name: string };
    lotQuantity: number;
    lotUnit: string;
    rawMaterialId: number;
}

export interface Containers{
    containerId: number;
    containerCode:string;
    containerName:string;
    containerVolume: number;
    activate: boolean;
}

export interface ItemInContainer {
    itemCode: number;
    containerId: number;
    dateTimeIn: number;
    dateTimeOut: number;
}

export interface ItemOfRawMaterial {
    itemRawId: number;
    itemCode: string; 
    quantity: number;
    unit: string;
    lotOfRawMaterialId: number | null;
    itemInContainerId: number | null;
}


export interface PlantFloorSection {
    sectionId: number;
    sectionCode: string; 
    name: string;       
    checkpointIds?: string[]; 
}


export class RawMaterialClass implements RawMaterial {
    constructor(
        public rawId: number,
        public name: string,
        public info: string
    ) {}
}

export class LotRawMaterialClass implements LotRawMaterial {
    constructor(
        public lotId: number,
        public id: string,
        public lotNumber: { name: string },
        public lotQuantity: number,
        public lotUnit: string,
        public rawMaterialId: number
    ) {}
}

export class ContainersClass implements Containers {
    constructor(
        public containerCode:string,
        public containerName:string,
        public containerVolume: number,
        public activate: boolean
    ) {}
}

export class ItemInContainerClass implements ItemInContainer {
    constructor(
        public containerCode: number,
        public materialId: string,
        public quantity: number,
    ) {}
}


export class ItemOfRawMaterialClass implements ItemOfRawMaterial {
    constructor(
        public itemRawId: number,
        public code: string,
        public quantity: number,
        public unit: string,
        public lotOfRawMaterialId: number,
        public itemInContainerId: number
    ) {}
}