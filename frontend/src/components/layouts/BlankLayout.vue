<template>
    <div class="blank-layout">
        <slot></slot>
    </div>
</template>
<style scoped lang="scss">
.blank-layout {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 100%;
    background-image: url('@/assets/images/background.png');
    background-size: cover;
}
</style>