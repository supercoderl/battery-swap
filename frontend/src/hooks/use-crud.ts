"use client";
import {
  useQuery, useMutation, useQueryClient,
  keepPreviousData, type UseQueryOptions,
} from "@tanstack/react-query";
import { toast } from "sonner";
import { getApiErrorMessage } from "@/lib/axios";
import type { CrudApi } from "@/lib/api-helpers";
import type { PagedResult } from "@/types";

export function createResourceHooks<TEntity, TCreate, TUpdate, TQuery extends object>(
  key: string,
  apiClient: CrudApi<TEntity, TCreate, TUpdate, TQuery>
) {
  function useList(query?: TQuery, options?: Partial<UseQueryOptions<PagedResult<TEntity>>>) {
    return useQuery({
      queryKey: [key, "list", query],
      queryFn: () => apiClient.list(query),
      placeholderData: keepPreviousData,
      ...options,
    });
  }

  function useDetail(id?: number) {
    return useQuery({
      queryKey: [key, "detail", id],
      queryFn: () => apiClient.get(id!),
      enabled: !!id,
    });
  }

  function useCreate() {
    const qc = useQueryClient();
    return useMutation({
      mutationFn: (dto: TCreate) => apiClient.create(dto),
      onSuccess: () => {
        qc.invalidateQueries({ queryKey: [key] });
        toast.success("Created successfully.");
      },
      onError: (e) => toast.error(getApiErrorMessage(e)),
    });
  }

  function useUpdate() {
    const qc = useQueryClient();
    return useMutation({
      mutationFn: ({ id, dto }: { id: number; dto: TUpdate }) => apiClient.update(id, dto),
      onSuccess: () => {
        qc.invalidateQueries({ queryKey: [key] });
        toast.success("Updated successfully.");
      },
      onError: (e) => toast.error(getApiErrorMessage(e)),
    });
  }

  function useDelete() {
    const qc = useQueryClient();
    return useMutation({
      mutationFn: (id: number) => apiClient.remove(id),
      onSuccess: () => {
        qc.invalidateQueries({ queryKey: [key] });
        toast.success("Deleted successfully.");
      },
      onError: (e) => toast.error(getApiErrorMessage(e)),
    });
  }

  return { useList, useDetail, useCreate, useUpdate, useDelete };
}
