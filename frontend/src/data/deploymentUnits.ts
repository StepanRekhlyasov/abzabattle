import sdImage from '@/assets/images/sd.png';
import mcImage from '@/assets/images/mc.png';
import { EntityType } from '@/types/entity';
import { Faction } from '@/types/session';

export type DeploymentUnitAsset = {
    type: EntityType;
    name: string;
    image: string;
    factions: Faction[];
};

export const DEPLOYMENT_UNIT_ASSETS: DeploymentUnitAsset[] = [
    { type: EntityType.StarDestroyer, name: 'Star Destroyer', image: sdImage, factions: [Faction.Imperial] },
    { type: EntityType.MonCalamari, name: 'Mon Calamari', image: mcImage, factions: [Faction.Rebel] },
];

export function getDeploymentUnitAsset(type: EntityType): DeploymentUnitAsset {
    const asset = DEPLOYMENT_UNIT_ASSETS.find(unit => unit.type === type);
    if (!asset) throw new Error(`Deployment unit asset not found for type: ${type}`);
    return asset;
}

export function getDeploymentUnitsByFaction(faction: Faction): DeploymentUnitAsset[] {
    return DEPLOYMENT_UNIT_ASSETS.filter(unit => unit.factions.includes(faction));
}
