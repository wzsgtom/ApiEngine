(() => {
    "use strict";

    const storeKey = "xhsoft.apiForm.root.v1";
    const methods = ["get", "post", "put", "patch", "delete", "options", "head"];
    const docCandidates = [
        "/swagger/医保API/swagger.json",
        `/swagger/${encodeURIComponent("医保API")}/swagger.json`,
        "/swagger/v1/swagger.json",
        "/swagger/default/swagger.json"
    ];
    const accessTokenHeader = "access-token";
    const authorizationHeader = "Authorization";
    const friendlyNames = {
        infno: "交易编号",
        user: "用户",
        patient: "患者信息",
        data: "业务数据"
    };

    const state = {
        spec: null,
        definitions: [],
        endpoints: [],
        activeDefinitionUrl: "",
        selected: null,
        formValue: null,
        detailOpen: {},
        authToken: "",
        requestHeaders: [],
        stored: {}
    };

    const el = {};
    let jsonInputTimer = 0;

    document.addEventListener("DOMContentLoaded", init);

    function init() {
        bindElements();
        state.stored = loadStore();
        state.authToken = state.stored.accessToken || state.stored.token || "";
        state.requestHeaders = normalizeHeaders(state.stored.headers);
        if (state.authToken) {
            removeRequestHeader(accessTokenHeader);
            upsertRequestHeader(authorizationHeader, normalizeAccessTokenValue(state.authToken), false);
        }
        el.baseUrl.value = state.stored.baseUrl || getOrigin();
        el.docUrl.value = state.stored.docUrl || docCandidates[0];
        bindEvents();
        renderRequestHeaders();
        enterEmptyState();
        discoverDefinitions();
    }

    function bindElements() {
        [
            "docStatus",
            "baseUrl",
            "docUrl",
            "reloadDoc",
            "definitionTabs",
            "endpointSelect",
            "sendRequest",
            "addHeader",
            "requestHeaderList",
            "parameterSection",
            "parameterRoot",
            "formRoot",
            "jsonPreview",
            "jsonStatus",
            "copyJson",
            "responseMeta",
            "copyResponse",
            "responseViewer"
        ].forEach((id) => {
            el[id] = document.getElementById(id);
        });
    }

    function bindEvents() {
        el.reloadDoc.addEventListener("click", discoverDefinitions);
        el.baseUrl.addEventListener("change", persistSettings);
        el.docUrl.addEventListener("change", persistSettings);
        el.addHeader.addEventListener("click", () => {
            syncHeaderInputs();
            state.requestHeaders.push({ name: "", value: "" });
            renderRequestHeaders();
            const inputs = el.requestHeaderList.querySelectorAll("[data-header-name]");
            inputs[inputs.length - 1]?.focus();
        });
        el.endpointSelect.addEventListener("change", () => {
            const endpoint = state.endpoints.find((item) => item.id === el.endpointSelect.value);
            if (endpoint) {
                selectEndpoint(endpoint);
            }
        });
        el.sendRequest.addEventListener("click", sendRequest);
        el.copyJson.addEventListener("click", () => copyText(el.jsonPreview.value, "入参 JSON 已复制。", setJsonStatus));
        el.copyResponse.addEventListener("click", () => copyText(el.responseViewer.textContent, "出参 JSON 已复制。", setResponseStatus));
        el.jsonPreview.addEventListener("input", () => {
            window.clearTimeout(jsonInputTimer);
            setJsonStatus("正在检查 JSON...", "muted");
            jsonInputTimer = window.setTimeout(syncJsonToForm, 300);
        });
    }

    async function discoverDefinitions() {
        setStatus("正在读取接口文档...");
        el.reloadDoc.disabled = true;
        let discoveredFromSwaggerUi = false;

        try {
            const html = await fetchText(new URL("/api/index.html", getOrigin()).toString());
            state.definitions = parseSwaggerDefinitions(html);
            discoveredFromSwaggerUi = state.definitions.length > 0;
        } catch {
            state.definitions = [];
        }

        if (state.definitions.length === 0) {
            state.definitions = unique([state.stored.docUrl, el.docUrl.value, ...docCandidates].filter(Boolean))
                .map((url) => ({ name: definitionNameFromUrl(url), url }));
        } else if (!discoveredFromSwaggerUi && el.docUrl.value.trim() && !state.definitions.some((item) => item.url === el.docUrl.value.trim())) {
            state.definitions = [
                { name: definitionNameFromUrl(el.docUrl.value.trim()), url: el.docUrl.value.trim() },
                ...state.definitions
            ];
        }

        const preferred = state.stored.definitionUrl || (discoveredFromSwaggerUi ? "" : (state.stored.docUrl || el.docUrl.value));
        const selected = (preferred && state.definitions.find((item) => item.url === preferred)) || state.definitions[0];
        renderDefinitionTabs();

        if (selected) {
            await loadDefinition(selected);
        } else {
            enterEmptyState();
            setStatus("未找到 Swagger 定义。");
        }
    }

    async function loadDefinition(definition) {
        state.activeDefinitionUrl = definition.url;
        el.docUrl.value = definition.url;
        renderDefinitionTabs();
        setStatus(`正在读取 ${definition.name}...`);
        el.reloadDoc.disabled = true;

        const errors = [];
        for (const candidate of [definition.url]) {
            const url = toAbsoluteUrl(candidate);
            if (!url) {
                continue;
            }

            try {
                const spec = await fetchJson(url);
                state.spec = spec;
                state.endpoints = parseEndpoints(spec);
                el.docUrl.value = candidate;
                updatePageTitle(spec, definition);
                persistSettings();
                renderEndpointOptions();
                selectEndpoint(state.endpoints.find((item) => item.bodyInfo) || state.endpoints[0]);
                setStatus(`已加载 ${definition.name}，共 ${state.endpoints.length} 个接口。`);
                return;
            } catch (error) {
                errors.push(`${candidate}: ${error.message}`);
            }
        }

        state.spec = null;
        state.endpoints = [];
        updatePageTitle(null, null);
        renderEndpointOptions();
        enterEmptyState();
        setStatus(`未读取到接口文档。${errors[0] || ""}`);
    }

    async function fetchText(url) {
        const response = await fetch(url, { cache: "no-store" });
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        return response.text();
    }

    async function fetchJson(url) {
        const response = await fetch(url, {
            headers: { Accept: "application/json" },
            cache: "no-store"
        });
        const text = await response.text();
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        try {
            return JSON.parse(text);
        } catch {
            throw new Error("返回内容不是 JSON");
        }
    }

    function parseEndpoints(spec) {
        const paths = spec.paths || {};
        const endpoints = [];

        Object.keys(paths).forEach((path) => {
            const pathItem = paths[path] || {};
            const sharedParameters = normalizeParameters(pathItem.parameters);
            methods.forEach((method) => {
                const operation = pathItem[method];
                if (!operation) {
                    return;
                }

                const title = operation.summary || operation.operationId || `${method.toUpperCase()} ${path}`;
                const group = Array.isArray(operation.tags) && operation.tags.length > 0 ? operation.tags[0] : firstPathPart(path);
                const parameters = [...sharedParameters, ...normalizeParameters(operation.parameters)];
                endpoints.push({
                    id: `${method.toUpperCase()} ${path}`,
                    method: method.toUpperCase(),
                    path,
                    group,
                    title,
                    operation,
                    parameters,
                    bodyInfo: getBodyInfo(operation)
                });
            });
        });

        return endpoints.sort((left, right) => {
            const groupSort = String(left.group).localeCompare(String(right.group), "zh-CN");
            if (groupSort !== 0) {
                return groupSort;
            }
            return String(left.path).localeCompare(String(right.path), "zh-CN", { numeric: true });
        });
    }

    function normalizeParameters(parameters) {
        return Array.isArray(parameters) ? parameters.map(resolveRef).filter(Boolean) : [];
    }

    function getBodyInfo(operation) {
        const requestBody = resolveRef(operation.requestBody);
        const content = requestBody && requestBody.content ? requestBody.content : null;
        if (!content) {
            return null;
        }

        const contentTypes = Object.keys(content);
        const contentType = contentTypes.find((item) => item.includes("json")) || contentTypes[0];
        const schema = content[contentType] && content[contentType].schema ? content[contentType].schema : null;
        return { contentType, required: Boolean(requestBody.required), schema };
    }

    function renderDefinitionTabs() {
        if (state.definitions.length === 0) {
            el.definitionTabs.innerHTML = "";
            return;
        }

        el.definitionTabs.innerHTML = state.definitions.map((definition) => `
            <button class="group-tab${definition.url === state.activeDefinitionUrl ? " active" : ""}" type="button" data-definition-url="${escapeHtml(definition.url)}">
                ${escapeHtml(definition.name)}
            </button>
        `).join("");

        el.definitionTabs.querySelectorAll("[data-definition-url]").forEach((button) => {
            button.addEventListener("click", () => {
                const definition = state.definitions.find((item) => item.url === button.dataset.definitionUrl);
                if (definition) {
                    loadDefinition(definition);
                }
            });
        });
    }

    function renderEndpointOptions() {
        const endpoints = state.endpoints;
        if (endpoints.length === 0) {
            el.endpointSelect.innerHTML = `<option value="">无接口</option>`;
            return;
        }

        el.endpointSelect.innerHTML = endpoints.map((endpoint) => `
            <option value="${escapeHtml(endpoint.id)}">${escapeHtml(endpoint.method)} ${escapeHtml(endpoint.title)} · ${escapeHtml(endpoint.path)}</option>
        `).join("");
    }

    function selectEndpoint(endpoint) {
        if (!endpoint) {
            enterEmptyState();
            return;
        }

        state.selected = endpoint;
        el.endpointSelect.value = endpoint.id;
        state.detailOpen = {};

        state.formValue = endpoint.bodyInfo && endpoint.bodyInfo.schema
            ? sampleFromSchema(endpoint.bodyInfo.schema, "body", new Set())
            : null;

        renderParameterForm(endpoint.parameters);
        renderBodyForm();
        updateJsonPreview();
        el.responseMeta.textContent = "尚未请求。";
        el.responseViewer.textContent = "尚未请求。";
    }

    function enterEmptyState() {
        state.selected = null;
        state.formValue = null;
        state.detailOpen = {};
        el.endpointSelect.innerHTML = `<option value="">无接口</option>`;
        el.parameterSection.classList.add("hidden");
        el.parameterRoot.innerHTML = "";
        el.formRoot.innerHTML = `<div class="empty-state">暂无接口。</div>`;
        el.jsonPreview.value = "";
    }

    function renderRequestHeaders() {
        const rows = state.requestHeaders.map((header, index) => `
            <div class="header-row">
                <input class="form-control form-control-sm" data-header-name="${index}" type="text" value="${escapeHtml(header.name)}" placeholder="Header" autocomplete="off" spellcheck="false">
                <input class="form-control form-control-sm" data-header-value="${index}" type="text" value="${escapeHtml(header.value)}" placeholder="值" autocomplete="off" spellcheck="false">
                <button class="btn btn-outline-secondary btn-sm" type="button" data-header-remove="${index}">删除</button>
            </div>
        `).join("");

        el.requestHeaderList.innerHTML = rows || `<div class="empty-state">未设置自定义请求头。</div>`;

        el.requestHeaderList.querySelectorAll("[data-header-name], [data-header-value]").forEach((input) => {
            input.addEventListener("input", () => {
                syncHeaderInputs();
                persistSettings();
            });
        });

        el.requestHeaderList.querySelectorAll("[data-header-remove]").forEach((button) => {
            button.addEventListener("click", () => {
                syncHeaderInputs();
                state.requestHeaders.splice(Number(button.dataset.headerRemove), 1);
                syncTokenFromHeaders();
                renderRequestHeaders();
                persistSettings();
            });
        });
    }

    function syncHeaderInputs() {
        if (!el.requestHeaderList) {
            return;
        }

        const next = [];
        el.requestHeaderList.querySelectorAll(".header-row").forEach((row) => {
            const name = row.querySelector("[data-header-name]")?.value.trim() || "";
            const value = row.querySelector("[data-header-value]")?.value.trim() || "";
            if (name || value) {
                next.push({ name, value });
            }
        });
        state.requestHeaders = next;
        syncTokenFromHeaders();
    }

    function syncTokenFromHeaders() {
        const tokenHeader = state.requestHeaders.find((header) => sameHeaderName(header.name, authorizationHeader));
        state.authToken = tokenHeader ? tokenHeader.value : "";
    }

    function normalizeHeaders(headers) {
        if (!Array.isArray(headers)) {
            return [];
        }

        return headers
            .map((header) => ({
                name: String(header.name || "").trim(),
                value: String(header.value || "").trim()
            }))
            .filter((header) => header.name || header.value);
    }

    function upsertRequestHeader(name, value, shouldRender) {
        const existing = state.requestHeaders.find((header) => sameHeaderName(header.name, name));
        if (existing) {
            existing.value = value;
        } else {
            state.requestHeaders.push({ name, value });
        }
        syncTokenFromHeaders();
        if (shouldRender) {
            renderRequestHeaders();
        }
    }

    function removeRequestHeader(name) {
        state.requestHeaders = state.requestHeaders.filter((header) => !sameHeaderName(header.name, name));
    }

    function renderParameterForm(parameters) {
        const visible = parameters.filter((parameter) => ["path", "query"].includes(parameter.in));
        if (visible.length === 0) {
            el.parameterSection.classList.add("hidden");
            el.parameterRoot.innerHTML = "";
            return;
        }

        el.parameterSection.classList.remove("hidden");
        el.parameterRoot.innerHTML = visible.map((parameter) => {
            const schema = materializeSchema(parameter.schema || {}, new Set());
            const title = strip(parameter.description || schema.description || friendlyNames[parameter.name] || parameter.name);
            const required = parameter.required ? `<span class="required">必填</span>` : "";
            return `
                <div class="field-row">
                    <div class="field-main">
                        <span class="field-title">${escapeHtml(title)}</span>
                        <span class="field-code">${escapeHtml(parameter.name)} · ${escapeHtml(parameter.in)}</span>
                        ${required}
                    </div>
                    <input data-param-name="${escapeHtml(parameter.name)}" data-param-in="${escapeHtml(parameter.in)}" type="text" value="${escapeHtml(sampleParameter(parameter))}" autocomplete="off" spellcheck="false">
                </div>`;
        }).join("");
    }

    function renderBodyForm() {
        if (!state.selected || !state.selected.bodyInfo || !state.selected.bodyInfo.schema) {
            el.formRoot.innerHTML = `<div class="empty-state">此接口没有请求体。</div>`;
            return;
        }

        el.formRoot.innerHTML = renderSchema(state.selected.bodyInfo.schema, state.formValue, [], {
            name: "body",
            title: "请求",
            required: state.selected.bodyInfo.required,
            root: true
        });

        bindFormEvents();
    }

    function renderSchema(schema, value, path, meta) {
        const actual = materializeSchema(schema, new Set());
        const type = schemaType(actual);

        if (type === "object") {
            return renderObject(actual, value, path, meta);
        }
        if (type === "array") {
            return renderArray(actual, value, path, meta);
        }
        return renderField(actual, value, path, meta);
    }

    function renderObject(schema, value, path, meta) {
        const properties = schema.properties || {};
        const required = new Set(schema.required || []);
        const body = Object.keys(properties).map((name) => {
            const childSchema = properties[name];
            const childActual = materializeSchema(childSchema, new Set());
            const childValue = value && typeof value === "object" ? value[name] : undefined;
            return renderSchema(childSchema, childValue, [...path, name], {
                name,
                title: fieldTitle(name, childSchema, childActual),
                required: required.has(name),
                root: false
            });
        }).join("");

        if (meta.root) {
            return body || `<div class="empty-state">请求体没有可生成的字段。</div>`;
        }

        const key = detailKey("object", path);
        const openAttr = detailOpenAttr(key, true);
        return `
            <details class="object-group" data-detail-key="${escapeHtml(key)}"${openAttr}>
                <summary class="group-heading">
                    <div>
                        <h3>${escapeHtml(meta.title || fieldTitle(meta.name, schema, schema))}</h3>
                        <code>${escapeHtml(path.join("."))}</code>
                    </div>
                    ${meta.required ? `<span class="required">必填</span>` : ""}
                </summary>
                <div class="object-content">${body || renderJsonFallback(value, path)}</div>
            </details>`;
    }

    function renderArray(schema, value, path, meta) {
        const items = Array.isArray(value) ? value : [];
        const title = meta.title || fieldTitle(meta.name, schema, schema);
        const pointer = pathToPointer(path);
        const groupKey = detailKey("array", path);
        const groupOpenAttr = detailOpenAttr(groupKey, true);
        const body = items.map((item, index) => `
            <details class="array-item" data-detail-key="${escapeHtml(detailKey("array-item", [...path, index]))}"${detailOpenAttr(detailKey("array-item", [...path, index]), true)}>
                <summary class="array-item-title">
                    <span>第 ${index + 1} 项</span>
                    <button type="button" data-array-remove="${escapeHtml(pointer)}" data-array-index="${index}">删除</button>
                </summary>
                ${renderSchema(schema.items || {}, item, [...path, index], {
                    name: String(index),
                    title: `${title} ${index + 1}`,
                    required: false,
                    root: schemaType(materializeSchema(schema.items || {}, new Set())) !== "object"
                })}
            </details>
        `).join("");

        return `
            <details class="array-group" data-detail-key="${escapeHtml(groupKey)}"${groupOpenAttr}>
                <summary class="array-heading">
                    <div>
                        <h3>${escapeHtml(title)}</h3>
                        <code>${escapeHtml(path.join("."))}</code>
                    </div>
                </summary>
                <div class="array-toolbar">
                    <button type="button" data-array-add="${escapeHtml(pointer)}">新增一项</button>
                </div>
                <div class="array-items">${body || `<div class="empty-state">当前为空。</div>`}</div>
            </details>`;
    }

    function renderField(schema, value, path, meta) {
        const pointer = pathToPointer(path);
        const title = meta.title || fieldTitle(meta.name, schema, schema);
        const type = schemaType(schema);
        const required = meta.required ? `<span class="required">必填</span>` : "";
        const hint = strip(schema.description || "");
        const code = path.join(".");

        return `
            <div class="field-row">
                <div class="field-main">
                    <span class="field-title">${escapeHtml(title)}</span>
                    <span class="field-code">${escapeHtml(code)}</span>
                    ${required}
                    ${hint && hint !== title ? `<span class="field-hint">${escapeHtml(hint)}</span>` : ""}
                </div>
                ${renderInput(schema, value, pointer, type)}
            </div>`;
    }

    function renderInput(schema, value, pointer, type) {
        if (Array.isArray(schema.enum) && schema.enum.length > 0) {
            return `<select data-pointer="${escapeHtml(pointer)}" data-type="${escapeHtml(type)}">${schema.enum.map((item) => `<option value="${escapeHtml(item)}" ${item === value ? "selected" : ""}>${escapeHtml(item)}</option>`).join("")}</select>`;
        }
        if (type === "boolean") {
            return `
                <select data-pointer="${escapeHtml(pointer)}" data-type="boolean">
                    <option value="false" ${value === false ? "selected" : ""}>false</option>
                    <option value="true" ${value === true ? "selected" : ""}>true</option>
                </select>`;
        }
        if (type === "integer" || type === "number") {
            return `<input data-pointer="${escapeHtml(pointer)}" data-type="${escapeHtml(type)}" type="number" value="${escapeHtml(value ?? "")}" autocomplete="off">`;
        }
        return `<input data-pointer="${escapeHtml(pointer)}" data-type="string" type="text" value="${escapeHtml(value ?? "")}" autocomplete="off" spellcheck="false">`;
    }

    function renderJsonFallback(value, path) {
        return `
            <div class="field-row">
                <div class="field-main">
                    <span class="field-title">JSON</span>
                    <span class="field-code">${escapeHtml(path.join("."))}</span>
                </div>
                <textarea data-pointer="${escapeHtml(pathToPointer(path))}" data-type="json">${escapeHtml(JSON.stringify(value || {}, null, 2))}</textarea>
            </div>`;
    }

    function bindFormEvents() {
        el.formRoot.querySelectorAll("details[data-detail-key]").forEach((details) => {
            state.detailOpen[details.dataset.detailKey] = details.open;
            details.addEventListener("toggle", () => {
                state.detailOpen[details.dataset.detailKey] = details.open;
            });
        });

        el.formRoot.querySelectorAll("[data-pointer]").forEach((input) => {
            input.addEventListener("input", () => {
                updateValueFromInput(input);
                updateJsonPreview();
            });
            input.addEventListener("change", () => {
                updateValueFromInput(input);
                updateJsonPreview();
            });
        });

        el.formRoot.querySelectorAll("[data-array-add]").forEach((button) => {
            button.addEventListener("click", () => {
                syncFormValue();
                const path = pointerToPath(button.dataset.arrayAdd);
                const schema = schemaAtPath(state.selected.bodyInfo.schema, path);
                const actual = materializeSchema(schema, new Set());
                const target = ensureArrayAtPath(state.formValue, path);
                target.push(sampleFromSchema(actual.items || {}, "item", new Set()));
                renderBodyForm();
                updateJsonPreview();
            });
        });

        el.formRoot.querySelectorAll("[data-array-remove]").forEach((button) => {
            button.addEventListener("click", (event) => {
                event.preventDefault();
                event.stopPropagation();
                syncFormValue();
                const path = pointerToPath(button.dataset.arrayRemove);
                const target = getValueAtPath(state.formValue, path);
                if (Array.isArray(target)) {
                    target.splice(Number(button.dataset.arrayIndex), 1);
                }
                renderBodyForm();
                updateJsonPreview();
            });
        });
    }

    function detailKey(type, path) {
        return `${type}:${pathToPointer(path) || "/"}`;
    }

    function detailOpenAttr(key, defaultOpen) {
        const open = Object.prototype.hasOwnProperty.call(state.detailOpen, key)
            ? state.detailOpen[key]
            : defaultOpen;
        return open ? " open" : "";
    }

    function updateValueFromInput(input) {
        const path = pointerToPath(input.dataset.pointer);
        let value = input.value;
        if (input.dataset.type === "integer") {
            value = value === "" ? null : parseInt(value, 10);
        } else if (input.dataset.type === "number") {
            value = value === "" ? null : Number(value);
        } else if (input.dataset.type === "boolean") {
            value = value === "true";
        } else if (input.dataset.type === "json") {
            try {
                value = JSON.parse(value || "null");
            } catch {
                return;
            }
        }

        if (path.length === 0) {
            state.formValue = value;
        } else {
            setValueAtPath(state.formValue, path, value);
        }
    }

    function syncFormValue() {
        el.formRoot.querySelectorAll("[data-pointer]").forEach(updateValueFromInput);
    }

    function updateJsonPreview() {
        el.jsonPreview.value = state.formValue === null || state.formValue === undefined
            ? ""
            : JSON.stringify(state.formValue, null, 2);
        setJsonStatus(state.formValue === null || state.formValue === undefined ? "当前接口无入参 JSON。" : "已根据表单更新。", "muted");
    }

    function syncJsonToForm() {
        if (!state.selected || !state.selected.bodyInfo || !state.selected.bodyInfo.schema) {
            setJsonStatus("当前接口没有请求体。", "muted");
            return;
        }

        const text = el.jsonPreview.value.trim();
        if (!text) {
            setJsonStatus("JSON 为空，表单暂不更新。", "danger");
            return;
        }

        let parsed;
        try {
            parsed = JSON.parse(text);
        } catch (error) {
            setJsonStatus(`JSON 格式错误：${error.message}`, "danger");
            return;
        }

        const compatibility = checkSchemaCompatibility(parsed, state.selected.bodyInfo.schema);
        if (!compatibility.ok) {
            setJsonStatus(compatibility.message, "danger");
            return;
        }

        state.formValue = parsed;
        renderBodyForm();
        setJsonStatus("已同步到表单。", "success");
    }

    function checkSchemaCompatibility(value, schema) {
        const actual = materializeSchema(schema, new Set());
        const type = schemaType(actual);
        if (type === "object" && !isPlainObject(value)) {
            return { ok: false, message: "当前接口入参需要 JSON 对象。" };
        }
        if (type === "array" && !Array.isArray(value)) {
            return { ok: false, message: "当前接口入参需要 JSON 数组。" };
        }
        return { ok: true, message: "" };
    }

    function setJsonStatus(message, type) {
        if (!el.jsonStatus) {
            return;
        }
        const colorClass = type === "success"
            ? "text-success"
            : type === "danger"
                ? "text-danger"
                : "text-muted";
        el.jsonStatus.className = `${colorClass} small`;
        el.jsonStatus.textContent = message;
    }

    function setResponseStatus(message) {
        if (el.responseMeta) {
            el.responseMeta.textContent = message;
        }
    }

    async function sendRequest() {
        if (!state.selected) {
            return;
        }

        syncFormValue();
        updateJsonPreview();
        const request = buildRequest();
        if (!request.ok) {
            el.responseMeta.textContent = request.error;
            return;
        }

        el.sendRequest.disabled = true;
        el.responseMeta.textContent = request.url;
        el.responseViewer.textContent = "请求中...";
        const started = performance.now();

        try {
            const response = await fetch(request.url, {
                method: request.method,
                headers: request.headers,
                body: request.body,
                credentials: "same-origin"
            });
            const text = await response.text();
            const elapsed = Math.round(performance.now() - started);
            const tokenUpdated = storeResponseToken(response);
            el.responseMeta.textContent = `HTTP ${response.status} · ${elapsed}ms${tokenUpdated ? " · token已更新" : ""}`;
            el.responseViewer.textContent = formatResponse(text);
        } catch (error) {
            el.responseMeta.textContent = "请求失败";
            el.responseViewer.textContent = error.message;
        } finally {
            el.sendRequest.disabled = false;
        }
    }

    function buildRequest() {
        const endpoint = state.selected;
        const method = endpoint.method;
        let path = endpoint.path;

        endpoint.parameters.filter((parameter) => parameter.in === "path").forEach((parameter) => {
            const input = findParameterInput(parameter);
            path = path.replaceAll(`{${parameter.name}}`, encodeURIComponent(input ? input.value : ""));
        });

        let url;
        try {
            const base = (el.baseUrl.value.trim() || getOrigin()).replace(/\/+$/, "");
            url = new URL(`${base}${path.startsWith("/") ? path : `/${path}`}`);
        } catch {
            return { ok: false, error: "服务地址无效。" };
        }

        endpoint.parameters.filter((parameter) => parameter.in === "query").forEach((parameter) => {
            const input = findParameterInput(parameter);
            if (input && input.value !== "") {
                url.searchParams.set(parameter.name, input.value);
            }
        });

        syncHeaderInputs();
        const headers = { Accept: "application/json, text/plain, */*" };
        let body = undefined;
        if (!["GET", "HEAD"].includes(method) && endpoint.bodyInfo && endpoint.bodyInfo.schema) {
            headers["Content-Type"] = endpoint.bodyInfo.contentType || "application/json";
            body = JSON.stringify(state.formValue || {});
        }
        applyRequestHeaders(headers);

        persistSettings();
        return { ok: true, method, url: url.toString(), headers, body };
    }

    function applyRequestHeaders(headers) {
        state.requestHeaders.forEach((header) => {
            if (header.name && header.value) {
                headers[header.name] = header.value;
            }
        });
    }

    function storeResponseToken(response) {
        const token = response.headers.get(accessTokenHeader);
        if (!token) {
            return false;
        }

        state.authToken = normalizeAccessTokenValue(token);
        removeRequestHeader(accessTokenHeader);
        upsertRequestHeader(authorizationHeader, state.authToken, true);
        persistSettings();
        return true;
    }

    function normalizeAccessTokenValue(value) {
        const text = String(value || "").trim();
        if (!text) {
            return "";
        }
        return /^[A-Za-z][A-Za-z0-9+.-]*\s+\S+/.test(text) ? text : `Bearer ${text}`;
    }

    function findParameterInput(parameter) {
        return Array.from(el.parameterRoot.querySelectorAll("[data-param-name]"))
            .find((input) => input.dataset.paramName === parameter.name && input.dataset.paramIn === parameter.in);
    }

    function sampleFromSchema(schema, name, seenRefs) {
        const actual = materializeSchema(schema, seenRefs);
        if (!actual) {
            return "";
        }
        if (actual.example !== undefined) {
            return actual.example;
        }
        if (actual.default !== undefined) {
            return actual.default;
        }
        if (Array.isArray(actual.enum) && actual.enum.length > 0) {
            return actual.enum[0];
        }

        const type = schemaType(actual);
        if (type === "object") {
            const output = {};
            Object.keys(actual.properties || {}).forEach((key) => {
                output[key] = sampleFromSchema(actual.properties[key], key, new Set(seenRefs));
            });
            return output;
        }
        if (type === "array") {
            return [sampleFromSchema(actual.items || {}, singular(name), new Set(seenRefs))];
        }
        if (type === "integer" || type === "number") {
            return 0;
        }
        if (type === "boolean") {
            return false;
        }
        return sampleString(name, actual);
    }

    function sampleParameter(parameter) {
        const schema = materializeSchema(parameter.schema || {}, new Set());
        if (parameter.example !== undefined) {
            return parameter.example;
        }
        if (schema.default !== undefined) {
            return schema.default;
        }
        return parameter.required ? sampleString(parameter.name, schema) : "";
    }

    function sampleString(name, schema) {
        const key = String(name || "").toLowerCase();
        if (schema && schema.format === "date-time") {
            return "2024-01-01 08:00:00";
        }
        if (schema && schema.format === "date") {
            return "2024-01-01";
        }
        if (key.includes("opter_type")) {
            return "1";
        }
        if (key.includes("opter_name")) {
            return "操作员";
        }
        if (key === "opter") {
            return "admin";
        }
        if (key.includes("flag")) {
            return "0";
        }
        if (key.includes("infno")) {
            return extractCode(state.selected ? state.selected.title : "") || "";
        }
        return "";
    }

    function materializeSchema(schema, seenRefs) {
        const resolved = resolveSchema(schema, seenRefs);
        if (!resolved) {
            return null;
        }
        if (Array.isArray(resolved.allOf)) {
            return resolved.allOf.reduce((merged, part) => mergeSchemas(merged, materializeSchema(part, new Set(seenRefs))), { ...resolved, allOf: undefined });
        }
        if (Array.isArray(resolved.oneOf) && resolved.oneOf.length > 0) {
            return materializeSchema(resolved.oneOf[0], seenRefs);
        }
        if (Array.isArray(resolved.anyOf) && resolved.anyOf.length > 0) {
            return materializeSchema(resolved.anyOf[0], seenRefs);
        }
        return resolved;
    }

    function resolveSchema(schema, seenRefs) {
        if (!schema) {
            return null;
        }
        if (schema.$ref) {
            if (seenRefs.has(schema.$ref)) {
                return {};
            }
            seenRefs.add(schema.$ref);
            return resolveSchema(resolveRef(schema), seenRefs);
        }
        return schema;
    }

    function mergeSchemas(left, right) {
        return {
            ...left,
            ...right,
            required: unique([...(left && left.required ? left.required : []), ...(right && right.required ? right.required : [])]),
            properties: {
                ...(left && left.properties ? left.properties : {}),
                ...(right && right.properties ? right.properties : {})
            }
        };
    }

    function schemaAtPath(schema, path) {
        let current = schema;
        for (const part of path) {
            const actual = materializeSchema(current, new Set());
            if (!actual) {
                return {};
            }
            if (schemaType(actual) === "array") {
                current = actual.items || {};
            } else {
                current = actual.properties ? actual.properties[part] : {};
            }
        }
        return current || {};
    }

    function schemaType(schema) {
        if (!schema) {
            return "string";
        }
        if (schema.type) {
            return Array.isArray(schema.type) ? schema.type[0] : schema.type;
        }
        if (schema.properties || schema.additionalProperties) {
            return "object";
        }
        if (schema.items) {
            return "array";
        }
        return "string";
    }

    function fieldTitle(name, rawSchema, actualSchema) {
        return strip((rawSchema && rawSchema.description) || (actualSchema && actualSchema.description) || friendlyNames[name] || name);
    }

    function resolveRef(value) {
        if (!value || !value.$ref || !value.$ref.startsWith("#/")) {
            return value;
        }
        return value.$ref.slice(2).split("/").reduce((current, key) => current && current[decodePointer(key)], state.spec);
    }

    function getValueAtPath(target, path) {
        return path.reduce((current, key) => current == null ? undefined : current[key], target);
    }

    function setValueAtPath(target, path, value) {
        let current = target;
        path.forEach((part, index) => {
            if (index === path.length - 1) {
                current[part] = value;
                return;
            }
            const nextPart = path[index + 1];
            if (current[part] == null) {
                current[part] = typeof nextPart === "number" ? [] : {};
            }
            current = current[part];
        });
    }

    function ensureArrayAtPath(target, path) {
        if (path.length === 0) {
            if (!Array.isArray(state.formValue)) {
                state.formValue = [];
            }
            return state.formValue;
        }

        let current = target;
        path.forEach((part, index) => {
            if (index === path.length - 1) {
                if (!Array.isArray(current[part])) {
                    current[part] = [];
                }
                current = current[part];
                return;
            }
            if (current[part] == null) {
                current[part] = {};
            }
            current = current[part];
        });
        return current;
    }

    function pathToPointer(path) {
        if (path.length === 0) {
            return "";
        }
        return `/${path.map((part) => String(part).replace(/~/g, "~0").replace(/\//g, "~1")).join("/")}`;
    }

    function pointerToPath(pointer) {
        if (!pointer) {
            return [];
        }
        return String(pointer).split("/").slice(1).map((part) => {
            const value = part.replace(/~1/g, "/").replace(/~0/g, "~");
            return /^\d+$/.test(value) ? Number(value) : value;
        });
    }

    function formatResponse(text) {
        if (!text) {
            return "(空响应)";
        }
        try {
            return JSON.stringify(JSON.parse(text), null, 2);
        } catch {
            return text;
        }
    }

    function setStatus(message) {
        el.docStatus.textContent = message;
        el.reloadDoc.disabled = false;
    }

    function updatePageTitle(spec, definition) {
        const title = strip(spec?.info?.title || definition?.name || "");
        document.title = title ? `${title} - 接口表单` : "接口表单";
    }

    function persistSettings() {
        state.stored = {
            baseUrl: el.baseUrl.value.trim(),
            docUrl: el.docUrl.value.trim(),
            definitionUrl: state.activeDefinitionUrl,
            accessToken: state.authToken,
            headers: state.requestHeaders
        };
        try {
            localStorage.setItem(storeKey, JSON.stringify(state.stored));
        } catch {
            // 本地存储不可用时忽略。
        }
    }

    function loadStore() {
        try {
            return JSON.parse(localStorage.getItem(storeKey) || "{}") || {};
        } catch {
            return {};
        }
    }

    function toAbsoluteUrl(value) {
        try {
            return new URL(value, getOrigin()).toString();
        } catch {
            return "";
        }
    }

    function getOrigin() {
        return window.location.origin && window.location.origin !== "null"
            ? window.location.origin
            : "http://localhost:8080";
    }

    function firstPathPart(path) {
        return String(path || "").split("/").filter(Boolean)[0] || "未分组";
    }

    function parseSwaggerDefinitions(html) {
        const match = String(html || "").match(/var\s+configObject\s*=\s*JSON\.parse\('((?:\\'|[^'])*)'\)/);
        if (!match) {
            return [];
        }

        try {
            const jsonText = match[1]
                .replace(/\\'/g, "'")
                .replace(/\\"/g, '"');
            const config = JSON.parse(jsonText);
            return Array.isArray(config.urls)
                ? config.urls.map((item) => ({ name: item.name || definitionNameFromUrl(item.url), url: item.url })).filter((item) => item.url)
                : [];
        } catch {
            return [];
        }
    }

    function definitionNameFromUrl(url) {
        const parts = String(url || "").split("/").filter(Boolean);
        if (parts.length >= 2) {
            return decodeURIComponent(parts[1]);
        }
        return "接口文档";
    }

    function extractCode(text) {
        const value = String(text || "");
        const bracket = value.match(/【([^】]+)】/);
        if (bracket) {
            return bracket[1];
        }
        const code = value.match(/\b\d{4}[A-Z]?\b/i);
        return code ? code[0] : "";
    }

    function strip(value) {
        return String(value || "")
            .replace(/<[^>]+>/g, " ")
            .replace(/\s+/g, " ")
            .trim();
    }

    function singular(value) {
        return String(value || "item").replace(/s$/i, "") || "item";
    }

    function decodePointer(value) {
        return decodeURIComponent(value.replace(/~1/g, "/").replace(/~0/g, "~"));
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function unique(values) {
        return [...new Set(values.filter((item) => item !== undefined && item !== null && item !== ""))];
    }

    function isPlainObject(value) {
        return Object.prototype.toString.call(value) === "[object Object]";
    }

    function sameHeaderName(left, right) {
        return String(left || "").toLowerCase() === String(right || "").toLowerCase();
    }

    async function copyText(value, successMessage, notify) {
        if (!value) {
            return;
        }
        try {
            await navigator.clipboard.writeText(value);
            notify(successMessage, "success");
        } catch {
            const input = document.createElement("textarea");
            input.value = value;
            document.body.appendChild(input);
            input.select();
            document.execCommand("copy");
            input.remove();
            notify(successMessage, "success");
        }
    }
})();
