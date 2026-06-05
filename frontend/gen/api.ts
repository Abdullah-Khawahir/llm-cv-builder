/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface ChatHistoryDto {
  messages?: ChatMessageDto[] | null;
}

export interface ChatMessageDto {
  role?: string | null;
  message?: string | null;
}

export interface ChatPromptRequest {
  prompt?: string | null;
}

export interface ChatSessionDto {
  /** @format uuid */
  id?: string;
  htmlDocument?: string | null;
  chatHistory?: ChatHistoryDto;
  /** @format int32 */
  version?: number | null;
}

export interface Error {
  message?: string | null;
}

export interface Thinking {
  message?: string | null;
}

export interface UserDto {
  id?: string | null;
  email?: string | null;
}

export interface UserLoginModel {
  email?: string | null;
  password?: string | null;
}

export interface UserRegisterModel {
  email?: string | null;
  password?: string | null;
}

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  HeadersDefaults,
  ResponseType,
} from "axios";
import axios from "axios";

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams
  extends Omit<AxiosRequestConfig, "data" | "params" | "url" | "responseType"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseType;
  /** request body */
  body?: unknown;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown>
  extends Omit<AxiosRequestConfig, "data" | "cancelToken"> {
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
  secure?: boolean;
  format?: ResponseType;
}

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public instance: AxiosInstance;
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private secure?: boolean;
  private format?: ResponseType;

  constructor({
    securityWorker,
    secure,
    format,
    ...axiosConfig
  }: ApiConfig<SecurityDataType> = {}) {
    this.instance = axios.create({
      ...axiosConfig,
      baseURL: axiosConfig.baseURL || "",
    });
    this.secure = secure;
    this.format = format;
    this.securityWorker = securityWorker;
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected mergeRequestParams(
    params1: AxiosRequestConfig,
    params2?: AxiosRequestConfig,
  ): AxiosRequestConfig {
    const method = params1.method || (params2 && params2.method);

    return {
      ...this.instance.defaults,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...((method &&
          this.instance.defaults.headers[
            method.toLowerCase() as keyof HeadersDefaults
          ]) ||
          {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected stringifyFormItem(formItem: unknown) {
    if (typeof formItem === "object" && formItem !== null) {
      return JSON.stringify(formItem);
    } else {
      return `${formItem}`;
    }
  }

  protected createFormData(input: Record<string, unknown>): FormData {
    if (input instanceof FormData) {
      return input;
    }
    return Object.keys(input || {}).reduce((formData, key) => {
      const property = input[key];
      const propertyContent: any[] =
        property instanceof Array ? property : [property];

      for (const formItem of propertyContent) {
        const isFileType = formItem instanceof Blob || formItem instanceof File;
        formData.append(
          key,
          isFileType ? formItem : this.stringifyFormItem(formItem),
        );
      }

      return formData;
    }, new FormData());
  }

  public request = async <T = any, _E = any>({
    secure,
    path,
    type,
    query,
    format,
    body,
    ...params
  }: FullRequestParams): Promise<AxiosResponse<T>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const responseFormat = format || this.format || undefined;

    if (
      type === ContentType.FormData &&
      body &&
      body !== null &&
      typeof body === "object"
    ) {
      body = this.createFormData(body as Record<string, unknown>);
    }

    if (
      type === ContentType.Text &&
      body &&
      body !== null &&
      typeof body !== "string"
    ) {
      body = JSON.stringify(body);
    }

    return this.instance.request({
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type ? { "Content-Type": type } : {}),
      },
      params: query,
      responseType: responseFormat,
      data: body,
      url: path,
    });
  };
}

/**
 * @title WebAPI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
 * @version 1.0
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthRegisterCreate
     * @request POST:/api/auth/register
     */
    authRegisterCreate: (data: UserRegisterModel, params: RequestParams = {}) =>
      this.request<UserDto, any>({
        path: `/api/auth/register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthLoginCreate
     * @request POST:/api/auth/login
     */
    authLoginCreate: (data: UserLoginModel, params: RequestParams = {}) =>
      this.request<UserDto, any>({
        path: `/api/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags CV
     * @name CvPreviewDetail
     * @request GET:/api/cv/preview/{id}
     */
    cvPreviewDetail: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/cv/preview/${id}`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsList
     * @request GET:/api/chat-sessions
     */
    chatSessionsList: (params: RequestParams = {}) =>
      this.request<ChatSessionDto[], any>({
        path: `/api/chat-sessions`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsCreate
     * @request POST:/api/chat-sessions
     */
    chatSessionsCreate: (params: RequestParams = {}) =>
      this.request<ChatSessionDto, any>({
        path: `/api/chat-sessions`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsDetail
     * @request GET:/api/chat-sessions/{id}
     */
    chatSessionsDetail: (id: string, params: RequestParams = {}) =>
      this.request<ChatSessionDto, any>({
        path: `/api/chat-sessions/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsStreamCreate
     * @request POST:/api/chat-sessions/{id}/stream
     */
    chatSessionsStreamCreate: (
      id: string,
      data: ChatPromptRequest,
      params: RequestParams = {},
    ) =>
      this.request<Thinking, Error>({
        path: `/api/chat-sessions/${id}/stream`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags ExternalAuth
     * @name ExternalAuthLoginGoogleList
     * @request GET:/api/ExternalAuth/login-google
     */
    externalAuthLoginGoogleList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/ExternalAuth/login-google`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ExternalAuth
     * @name ExternalAuthGoogleCallbackList
     * @request GET:/api/ExternalAuth/google-callback
     */
    externalAuthGoogleCallbackList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/ExternalAuth/google-callback`,
        method: "GET",
        ...params,
      }),
  };
}
