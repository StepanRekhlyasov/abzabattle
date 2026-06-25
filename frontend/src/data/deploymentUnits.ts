import sdImage from '@/assets/images/sd.png';
import dsImage from '@/assets/images/ds.png';
import mcImage from '@/assets/images/mc.png';
import tfImage from '@/assets/images/tf.png';
import gzImage from '@/assets/images/gz.png';
import nfImage from '@/assets/images/nf.png';
import xwImage from '@/assets/images/xw.png';
import uwImage from '@/assets/images/uw.png';
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
    { type: EntityType.DeathStar, name: 'Death Star', image: dsImage, factions: [Faction.Imperial] },
    { type: EntityType.TieFighter, name: 'Tie-Fighter', image: tfImage, factions: [Faction.Imperial] },
    { type: EntityType.GozantiClassCruiser, name: 'Gozanti Class Cruiser', image: gzImage, factions: [Faction.Imperial] },
    { type: EntityType.MonCalamari, name: 'Mon Calamari', image: mcImage, factions: [Faction.Rebel] },
    { type: EntityType.NebulonFrigate, name: 'Nebulon Frigate', image: nfImage, factions: [Faction.Rebel] },
    { type: EntityType.XWing, name: 'X-Wing', image: xwImage, factions: [Faction.Rebel] },
    { type: EntityType.UWing, name: 'U-Wing', image: uwImage, factions: [Faction.Rebel] },
];

export function getDeploymentUnitAsset(type: EntityType): DeploymentUnitAsset {
    const asset = DEPLOYMENT_UNIT_ASSETS.find(unit => unit.type === type);
    if (!asset) throw new Error(`Deployment unit asset not found for type: ${type}`);
    return asset;
}

export function getDeploymentUnitsByFaction(faction: Faction): DeploymentUnitAsset[] {
    return DEPLOYMENT_UNIT_ASSETS.filter(unit => unit.factions.includes(faction));
}
