import React, { useState, useEffect } from 'react';
import { View, StyleSheet } from 'react-native';
import { useApp } from '../lib/hooks';
import { StreamSession } from '../lib/types';
import Log from '../CommonJs/Log';
import { delayAsync } from '../CommonJs/utilTs';
import Busy from './Busy';

// @ts-ignore
import Video from 'react-native-video';

interface StreamProps
{
    locationId?:string;
    streamId?:number;
}

export default function Stream({
    locationId,
    streamId
}:StreamProps){

    const {config,http,locations}=useApp();

    const location=locations.getLocation(locationId);

    const [session,setSession]=useState<null|StreamSession>(null);

    useEffect(()=>{

        if(!location || streamId===undefined){
            return;
        }
        let m=true;
        let session:StreamSession|null=null;
        const run=async ()=>{
            try{
                session=await http.getAsync<StreamSession>(`${location.ApiBaseUrl}api/Stream/${streamId}/Open`,{clientToken:location.Token});
                if(!m){return;}
                setSession(session);
            }catch(ex){
                Log.error('Unable to start streaming session');
                return;
            }

            try{
                while(m){
                    await delayAsync(session.TTLSeconds*1000*0.75);
                    if(!m){break;}
                    await http.getAsync(`${location.ApiBaseUrl}api/Stream/${streamId}/Extend/${session.Id}`,{clientToken:location.Token,sessionToken:session.Token})
                }
            }catch(ex){
                Log.error('Unable to extend streaming session');
                return;
            }
        }
        run();

        const close=async ()=>{
            try{
                if(session){
                    await http.getAsync(`${location.ApiBaseUrl}api/Stream/${streamId}/Close/${session.Id}`,{clientToken:location.Token,sessionToken:session.Token});
                }
            }catch{}
        }

        return ()=>{m=false;close()}


    },[streamId,config,http,location]);

    return (
        <View style={styles.root}>
            {session && location?
                <Video
                    style={styles.video}
                    source={{uri:location.ApiBaseUrl+session.Uri}}/>
                :
                <Busy/>
            }
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    },
    video:{
       flex:1
    }
});
