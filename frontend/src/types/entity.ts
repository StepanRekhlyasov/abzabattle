export enum EntityType {
    Letter = 'letter',
    Empty = 'empty',
}

interface BaseEntity<T extends EntityType> {
    id: string;
    type: T;
    shouldFetch?: boolean;
    content?: string;
}

export interface LetterEntity extends BaseEntity<EntityType.Letter> {
}

export interface EmptyEntity extends BaseEntity<EntityType.Empty> {
}

export type Entity<T extends EntityType = EntityType> = EntityTypeMap[T & keyof EntityTypeMap];

type EntityTypeMap = {
    [EntityType.Letter]: LetterEntity;
    [EntityType.Empty]: EmptyEntity;
};
