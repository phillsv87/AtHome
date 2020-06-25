import HsConfig from "./HsConfig";
import History from "../CommonJs/History-rn";
import Http from "../CommonJs/Http";
import AsyncObjStore from "../CommonJs/AsyncObjStore";
import AsyncObjStoreRn from "../CommonJs/AsyncObjStore-rn";
import LocationManager from "./LocationManager";
import { NativeDevice } from "./NativeDevice";

export default class HsApp
{

    public readonly app:HsApp;

    public readonly config:HsConfig;



    // CommonJs services

    public readonly history:History;

    public readonly store:AsyncObjStore;

    public readonly http:Http;



    // App specific services

    public readonly device:NativeDevice;

    public readonly locations:LocationManager;

    constructor(config:HsConfig)
    {
        this.app=this;
        this.config=config;
        this.history=new History();
        this.history.logChanges=true;
        this.store=new AsyncObjStoreRn('home-secure');
        this.http=new Http('',true);
        this.device=new NativeDevice(this);
        this.locations=new LocationManager(this);
    }

    public async initAsync():Promise<boolean>
    {
        await this.device.initAsync();
        await this.locations.initAsync();
        return true;
    }

    public dispose()
    {
        this.device.dispose();
    }
}