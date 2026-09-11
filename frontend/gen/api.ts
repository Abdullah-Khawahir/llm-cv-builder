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

/** @format int32 */
export enum PaymentResultStatus {
  Value0 = 0,
  Value1 = 1,
  Value2 = 2,
}

/** @format int32 */
export enum PaymentEventType {
  Value0 = 0,
  Value1 = 1,
  Value2 = 2,
  Value3 = 3,
  Value4 = 4,
  Value5 = 5,
  Value6 = 6,
  Value7 = 7,
  Value8 = 8,
}

export interface Activities {
  id?: string | null;
  object?: string | null;
  /** @format int64 */
  created?: number;
  status?: string | null;
  currency?: string | null;
  /** @format double */
  amount?: number;
  remarks?: string | null;
  txn_id?: string | null;
}

export interface Authentication {
  acsEci?: string | null;
  transaction_status?: string | null;
  id?: string | null;
}

export interface Card {
  object?: string | null;
  first_six?: string | null;
  first_eight?: string | null;
  scheme?: string | null;
  brand?: string | null;
  last_four?: string | null;
}

export interface CardSecurity {
  code?: string | null;
  message?: string | null;
}

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

export interface ChatSessionDetailsDto {
  /** @format uuid */
  id?: string;
  title?: string | null;
  htmlDocument?: string | null;
  chatHistory?: ChatHistoryDto;
  /** @format int32 */
  version?: number | null;
}

export interface ChatSessionListItemDto {
  /** @format uuid */
  id?: string;
  title?: string | null;
  /** @format date-time */
  updatedAt?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface Customer {
  id?: string | null;
  first_name?: string | null;
  email?: string | null;
}

export interface Date {
  /** @format int64 */
  created?: number;
  /** @format int64 */
  completed?: number;
  /** @format int64 */
  transaction?: number;
}

export interface ErrorEvent {
  message?: string | null;
}

export interface Expiry {
  /** @format int32 */
  period?: number;
  type?: string | null;
}

export interface Gateway {
  response?: Response;
}

export interface Intent {
  id?: string | null;
}

export interface JwtTokenResponse {
  token?: string | null;
}

export interface Merchant {
  country?: string | null;
  currency?: string | null;
  id?: string | null;
}

export interface Payment {
  /** @format uuid */
  id?: string;
  /** @format double */
  amount?: number;
  currency?: string | null;
  provider?: string | null;
  providerRef?: string | null;
  failureReason?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string | null;
  events?: PaymentEvent[] | null;
}

export interface PaymentEvent {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  paymentId?: string;
  type?: PaymentEventType;
  message?: string | null;
  rawPayload?: string | null;
  /** @format date-time */
  createdAt?: string;
}

export interface PaymentRequest {
  token?: string | null;
  /** @format double */
  amount?: number;
  currency?: string | null;
  firstName?: string | null;
  email?: string | null;
  provider?: string | null;
}

export interface PaymentResult {
  status?: PaymentResultStatus;
  transactionId?: string | null;
  redirectUrl?: string | null;
  error?: string | null;
  provider?: string | null;
}

export interface Post {
  status?: string | null;
  url?: string | null;
}

export interface Protect {
  id?: string | null;
  is_in_exclusion_list?: boolean;
  status?: string | null;
}

export interface Receipt {
  id?: string | null;
  email?: boolean;
  sms?: boolean;
}

export interface Redirect {
  status?: string | null;
  url?: string | null;
}

export interface Reference {
  track?: string | null;
  payment?: string | null;
  acquirer?: string | null;
  gateway?: string | null;
}

export interface Response {
  code?: string | null;
  message?: string | null;
}

export interface Security {
  threeDSecure?: ThreeDSecure;
}

export interface Source {
  object?: string | null;
  type?: string | null;
  payment_type?: string | null;
  channel?: string | null;
  id?: string | null;
  on_file?: boolean;
  payment_method?: string | null;
}

export interface TapWebhookBody {
  id?: string | null;
  object?: string | null;
  live_mode?: boolean;
  customer_initiated?: boolean;
  api_version?: string | null;
  method?: string | null;
  status?: string | null;
  /** @format double */
  amount?: number;
  currency?: string | null;
  threeDSecure?: boolean;
  card_threeDSecure?: boolean;
  save_card?: boolean;
  merchant_id?: string | null;
  product?: string | null;
  description?: string | null;
  transaction?: Transaction;
  reference?: Reference;
  response?: Response;
  card_security?: CardSecurity;
  security?: Security;
  gateway?: Gateway;
  card?: Card;
  receipt?: Receipt;
  customer?: Customer;
  merchant?: Merchant;
  source?: Source;
  redirect?: Redirect;
  post?: Post;
  authentication?: Authentication;
  activities?: Activities[] | null;
  auto_reversed?: boolean;
  intent?: Intent;
  protect?: Protect;
  initiator?: string | null;
}

export interface Thinking {
  message?: string | null;
}

export interface ThreeDSecure {
  id?: string | null;
  status?: string | null;
}

export interface TitleModel {
  title?: string | null;
}

export interface Transaction {
  authorization_id?: string | null;
  timezone?: string | null;
  created?: string | null;
  expiry?: Expiry;
  asynchronous?: boolean;
  /** @format double */
  amount?: number;
  currency?: string | null;
  date?: Date;
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
      this.request<JwtTokenResponse, any>({
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
      this.request<ChatSessionListItemDto[], any>({
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
      this.request<ChatSessionDetailsDto, any>({
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
      this.request<ChatSessionDetailsDto, any>({
        path: `/api/chat-sessions/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsDelete
     * @request DELETE:/api/chat-sessions/{id}
     */
    chatSessionsDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/chat-sessions/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags ChatSession
     * @name ChatSessionsPartialUpdate
     * @request PATCH:/api/chat-sessions/{id}
     */
    chatSessionsPartialUpdate: (
      id: string,
      data: TitleModel,
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/api/chat-sessions/${id}`,
        method: "PATCH",
        body: data,
        type: ContentType.Json,
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
      this.request<Thinking, ErrorEvent>({
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

    /**
     * No description
     *
     * @tags Payments
     * @name PaymentsList
     * @request GET:/api/payments
     */
    paymentsList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/payments`,
        method: "GET",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Payments
     * @name PaymentsCreate
     * @request POST:/api/payments
     */
    paymentsCreate: (data: PaymentRequest, params: RequestParams = {}) =>
      this.request<PaymentResult, PaymentResult>({
        path: `/api/payments`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Payments
     * @name PaymentsDetail
     * @request GET:/api/payments/{id}
     */
    paymentsDetail: (id: string, params: RequestParams = {}) =>
      this.request<Payment, void>({
        path: `/api/payments/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags TapWebhook
     * @name WebhooksTapCreate
     * @request POST:/api/webhooks/tap
     */
    webhooksTapCreate: (data: TapWebhookBody, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/webhooks/tap`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
}
