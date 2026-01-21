window.railScrollByPage = (el, dir = 1) => {
    if (!el) return;
    const amount = Math.max(200, el.clientWidth * 0.9);
    el.scrollBy({ left: dir * amount, behavior: "smooth" });
};

window.railInitButtons = (trackId, leftId, rightId) => {
    const track = document.getElementById(trackId);
    const left = document.getElementById(leftId);
    const right = document.getElementById(rightId);
    if (!track || !left || !right) return;

    const update = () => {
        const maxScroll = track.scrollWidth - track.clientWidth;
        const atStart = track.scrollLeft <= 1;
        const atEnd = track.scrollLeft >= maxScroll - 1;
        left.classList.toggle("is-hidden", maxScroll <= 0 || atStart);
        right.classList.toggle("is-hidden", maxScroll <= 0 || atEnd);
    };

    update();
    track.addEventListener("scroll", update, { passive: true });
    window.addEventListener("resize", update);
};
