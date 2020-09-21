export interface Message {
    propertyName?: string;
    body: string;
    level: 'Info' | 'Warning' | 'Error';
}

export interface CommandResult<T> {
    isSuccess: boolean;
    identity?: string;
    aggregateRootId?: string;
    title?: string | undefined;
    error?: string;
    messages: Message[];
    item?: T;
}

//export interface CommandModel<T> {
//    isBusy: boolean;
//    result?: CommandResult<T>;
//}

//export type ItemState = 'Unchanged' | 'New' | 'Changed';

//export interface ItemModel<T> extends CommandModel<T> {
//    state: ItemState;
//    item: T;
//}

//export interface DataModel<T> {
//    isBusy: boolean;
//    error: string;
//    items: ItemModel<T>[];
//    count: number;
//    discard: (item: T) => void;
//}
