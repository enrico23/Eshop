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

    // Re-init safety: remove previous listeners for this track
    track._railAbort?.abort();
    track._railAbort = new AbortController();
    const { signal } = track._railAbort;

    const update = () => {
        const maxScroll = track.scrollWidth - track.clientWidth;
        const atStart = track.scrollLeft <= 1;
        const atEnd = track.scrollLeft >= maxScroll - 1;

        left.classList.toggle("is-hidden", maxScroll <= 0 || atStart);
        right.classList.toggle("is-hidden", maxScroll <= 0 || atEnd);
    };

    update();
    requestAnimationFrame(update);
    requestAnimationFrame(update);

    // `load` is fine, but only if you don't accumulate handlers (signal solves that)
    window.addEventListener("load", update, { signal });
    track.addEventListener("scroll", update, { passive: true, signal });
    window.addEventListener("resize", update, { signal });
};