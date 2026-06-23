import type { BattleSector } from "@/types/map";
import type { Entity } from "@/types/entity";
import { h } from "vue";

export const useSector = <T extends Entity>(sector: BattleSector<T>) => {
    return {
        content: h('div', { class: 'sector-content' }, [
            h('div', { class: 'sector-content-item' }, sector.entity.content),
        ])
    };
}
