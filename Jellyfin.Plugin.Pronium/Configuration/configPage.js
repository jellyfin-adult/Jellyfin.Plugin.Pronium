define(['baseView', 'loading', 'emby-input', 'emby-button', 'emby-checkbox', 'emby-scroller'], function (BaseView, loading) {
    'use strict';

    const guid = '172ff6fc-2297-4c96-979a-e0ad632b6120';

    function loadPage(page, config) {
        page.querySelectorAll('#proniumConfigForm input[type=text]').forEach((t) => {
            t.value = config[t.id] ?? '';
        });
        page.querySelectorAll('#proniumConfigForm select').forEach((t) => {
            t.value = config[t.id] ?? t.querySelector('option').value;
        });
        page.querySelectorAll('#proniumConfigForm input[type=checkbox]').forEach((t) => {
            t.checked = !!config[t.id];
        });
        loading.hide();
    }

    function onSubmit(e) {
        const form = this;

        e.preventDefault();
        loading.show();

        ApiClient.getPluginConfiguration(guid).then(function (config) {
            page.querySelectorAll('#proniumConfigForm input[type=text]').forEach((t) => {
                config[t.id] = t.value;
            });
            page.querySelectorAll('#proniumConfigForm select').forEach((t) => {
                config[t.id] = t.value;
            });
            page.querySelectorAll('#proniumConfigForm input[type=checkbox]').forEach((t) => {
                config[t.id] = t.checked;
            });

            ApiClient.updatePluginConfiguration(guid, config).then(Dashboard.processServerConfigurationUpdateResult);
        });

        return false;
    }

    function getConfig() {
        return ApiClient.getPluginConfiguration(guid);
    }

    function View(view, params) {
        BaseView.apply(this, arguments);

        view.querySelector('form').addEventListener('submit', onSubmit);
    }

    Object.assign(View.prototype, BaseView.prototype);

    View.prototype.onResume = function (options) {
        const page = this.view;

        BaseView.prototype.onResume.apply(this, arguments);
        loading.show();

        getConfig().then(function (response) {
            loadPage(page, response);
        });
    };

    return View;
});
