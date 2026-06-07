import { api } from "@/lib/axios";
import type { ApiResponse, PagedResult } from "@/types";

/** Unwraps the standard ApiResponse envelope. */
export async function unwrap<T>(promise: Promise<{ data: ApiResponse<T> }>): Promise<T> {
  const res = await promise;
  return res.data.data;
}

/** Strips undefined / empty values so they don't appear in the query string. */
export function cleanParams(params: object): Record<string, unknown> {
  return Object.fromEntries(
    Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== "")
  );
}

export interface CrudApi<TEntity, TCreate, TUpdate, TQuery> {
  list: (query?: TQuery) => Promise<PagedResult<TEntity>>;
  get: (id: number) => Promise<TEntity>;
  create: (dto: TCreate) => Promise<TEntity>;
  update: (id: number, dto: TUpdate) => Promise<TEntity>;
  remove: (id: number) => Promise<void>;
}

export function createCrudApi<TEntity, TCreate, TUpdate, TQuery extends object>(
  resource: string
): CrudApi<TEntity, TCreate, TUpdate, TQuery> {
  return {
    list: (query) =>
      unwrap<PagedResult<TEntity>>(
        api.get(`/${resource}`, { params: cleanParams((query ?? {}) as Record<string, unknown>) })
      ),
    get: (id) => unwrap<TEntity>(api.get(`/${resource}/${id}`)),
    create: (dto) => unwrap<TEntity>(api.post(`/${resource}`, dto)),
    update: (id, dto) => unwrap<TEntity>(api.put(`/${resource}/${id}`, dto)),
    remove: async (id) => {
      await api.delete(`/${resource}/${id}`);
    },
  };
}
