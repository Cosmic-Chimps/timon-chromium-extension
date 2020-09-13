function setBadgeText(text) {
    chrome.browserAction.setBadgeText({ text: text });
}

function getTabUrl() {
    return new Promise((resolve, reject) => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            let url = tabs[0].url;
            resolve(url);
        });
    });
}

export { setBadgeText, getTabUrl };
