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
    DeathStar = 'death-star',
    MonCalamari = 'mon-calamari',
    TieFighter = 'tie-fighter',
    GozantiClassCruiser = 'gozanti-class-cruiser',
    SpaceMine = 'space-mine',
    NebulonFrigate = 'nebulon-frigate',
    XWing = 'x-wing',
    UWing = 'u-wing',
}

interface BaseEntity<T extends EntityType> {
    type: T;
    id?: string;
    content?: string;
    rotation?: EntityRotation;
    reactor?: boolean;
}

export interface LetterEntity extends BaseEntity<EntityType.Letter> {}
export interface EmptyEntity extends BaseEntity<EntityType.Empty> {}

export type RotatableEntity<T extends EntityType> = BaseEntity<T> & { rotation: EntityRotation };
export type StarDestroyerEntity = RotatableEntity<EntityType.StarDestroyer>;
export type DeathStarEntity = RotatableEntity<EntityType.DeathStar>;
export type MonCalamariEntity = RotatableEntity<EntityType.MonCalamari>;
export type TieFighterEntity = RotatableEntity<EntityType.TieFighter>;
export type GozantiClassCruiserEntity = RotatableEntity<EntityType.GozantiClassCruiser>;
export type SpaceMineEntity = RotatableEntity<EntityType.SpaceMine>;
export type NebulonFrigateEntity = RotatableEntity<EntityType.NebulonFrigate>;
export type XWingEntity = RotatableEntity<EntityType.XWing>;
export type UWingEntity = RotatableEntity<EntityType.UWing>;

export type Entity<T extends EntityType = EntityType> = EntityTypeMap[T & keyof EntityTypeMap];

type EntityTypeMap = {
    [EntityType.Letter]: LetterEntity;
    [EntityType.Empty]: EmptyEntity;
    [EntityType.StarDestroyer]: StarDestroyerEntity;
    [EntityType.DeathStar]: DeathStarEntity;
    [EntityType.MonCalamari]: MonCalamariEntity;
    [EntityType.TieFighter]: TieFighterEntity;
    [EntityType.GozantiClassCruiser]: GozantiClassCruiserEntity;
    [EntityType.SpaceMine]: SpaceMineEntity;
    [EntityType.NebulonFrigate]: NebulonFrigateEntity;
    [EntityType.XWing]: XWingEntity;
    [EntityType.UWing]: UWingEntity;
};

export type EntityDefinition = {
    ptsCost: number;
    footprint: CellOffset[];
    content?: string;
};

export type MapPosition = { x: number; y: number };
