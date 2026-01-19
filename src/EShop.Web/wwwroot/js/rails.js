window.railScrollByPage = (el, dir = 1) => {
    if (!el) return;
    const amount = Math.max(200, el.clientWidth * 0.9);
    el.scrollBy({ left: dir * amount, behavior: "smooth" });
};