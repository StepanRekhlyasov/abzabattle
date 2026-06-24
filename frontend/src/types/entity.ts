export enum EntityType {
    Letter = 'letter',
    Empty = 'empty',
    StarDestroyer = 'star-destroyer',
}

interface BaseEntity<T extends EntityType> {
    type: T;
    id?: string;
    shouldFetch?: boolean;
    content?: string;
}

export interface LetterEntity extends BaseEntity<EntityType.Letter> {
}

export interface EmptyEntity extends BaseEntity<EntityType.Empty> {
}

export interface StarDestroyerEntity extends BaseEntity<EntityType.StarDestroyer> {
}

export type Entity<T extends EntityType = EntityType> = EntityTypeMap[T & keyof EntityTypeMap];

type EntityTypeMap = {
    [EntityType.Letter]: LetterEntity;
    [EntityType.Empty]: EmptyEntity;
    [EntityType.StarDestroyer]: StarDestroyerEntity;
};
