import HsApp from "./HsApp";
import { useContext, useState, useMemo, useEffect } from "react";
import React from "react";
import HsConfig from "./HsConfig";
import { getCurrentTime } from "../CommonJs/commonUtils";
import { delayAsync } from "../CommonJs/utilTs";
import Log from "../CommonJs/Log";

export const HsAppContext=React.createContext<HsApp|null>(null);

// Workaround to fix hot reloads
let defaultUseApp:HsApp|null=null;
export function useApp():HsApp
{
    const app=useContext(HsAppContext);
    if(app){
        defaultUseApp=app;
    }
    return ((app||defaultUseApp) as HsApp);
}

export function useCreateApp(config:HsConfig):[HsApp|null,number]
{
    const [app,setApp]=useState<HsApp|null>(null);
    const [attempts,setAttempts]=useState(0);
    const startTime=useMemo(()=>getCurrentTime(),[]);

    useEffect(()=>{

        let m=true;
        const app=new HsApp(config);
        console.log('Create HsApp',attempts,app);

        const initAsync=async ()=>{
            let success=false;
            const tryAgin=attempts>10?20:8;
            if(attempts){
                await delayAsync(tryAgin*1000);
            }
            if(!m)return;
            try{
                
                success=await app.initAsync();
                if(!m)return;

            }catch(ex){
                Log.error('App Load Failed. Trying again in '+tryAgin+' seconds',ex);
            }
            if(success){
                const diff=config.MinSplashMs-(getCurrentTime()-startTime);
                if(diff>0){
                    await delayAsync(diff);
                    if(!m)return;
                }
                setApp(app);
            }else{
                setAttempts(v=>v+1);
            }
        }
        initAsync();


        return ()=>{
            m=false;
            try{
                app.dispose();
            }catch(ex){
                Log.error('Dispose app failed',ex);
            }
        }

    },[startTime,attempts,config]);

    return [app,attempts];
}