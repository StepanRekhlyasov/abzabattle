export enum EntityRotation {
    R0 = 0,
    R90 = 90,
    R180 = 180,
    R270 = 270,
}

export type CellOffset = { x: number; y: number };

export enum EntityType {
    Letter = 'letter',
    Empty = 'empty',
    StarDestroyer = 'star-destroyer',
    MonCalamari = 'mon-calamari',
}

interface BaseEntity<T extends EntityType> {
    type: T;
    id?: string;
    content?: string;
    rotation?: EntityRotation;
}

export interface LetterEntity extends BaseEntity<EntityType.Letter> {}
export interface EmptyEntity extends BaseEntity<EntityType.Empty> {}

export type RotatableEntity<T extends EntityType> = BaseEntity<T> & { rotation: EntityRotation };
export type StarDestroyerEntity = RotatableEntity<EntityType.StarDestroyer>;
export type MonCalamariEntity = RotatableEntity<EntityType.MonCalamari>;

export type Entity<T extends EntityType = EntityType> = EntityTypeMap[T & keyof EntityTypeMap];

type EntityTypeMap = {
    [EntityType.Letter]: LetterEntity;
    [EntityType.Empty]: EmptyEntity;
    [EntityType.StarDestroyer]: StarDestroyerEntity;
    [EntityType.MonCalamari]: MonCalamariEntity;
};

export type EntityDefinition = {
    ptsCost: number;
    footprint: CellOffset[];
    content?: string;
};

export type MapPosition = { x: number; y: number };
